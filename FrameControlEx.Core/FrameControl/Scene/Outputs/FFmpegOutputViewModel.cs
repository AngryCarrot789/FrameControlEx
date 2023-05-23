using System;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    // https://github.com/denesik/ffmpeg_video_encoder/blob/main/ffmpeg_encode.cpp
    public class FFmpegOutputViewModel : AVOutputViewModel {
        private volatile bool isRunning;
        public bool IsRunning {
            get => this.isRunning;
            internal set {
                this.isRunning = value;
                this.RaisePropertyChanged();
            }
        }

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set {
                if (this.IsRunning)
                    return;

                this.RaisePropertyChanged(ref this.filePath, value);
            }
        }

        public AsyncRelayCommand StartCommand { get; }
        public AsyncRelayCommand StopCommand { get; }
        public AsyncRelayCommand ToggleStartCommand { get; }

        public AsyncRelayCommand SelectOutputFileCommand { get; }

        private Context mContext;
        private Params mParams;
        private bool mIsOpen;

        public FFmpegOutputViewModel() {
            this.StartCommand = new AsyncRelayCommand(this.StartAction, () => !this.IsRunning && this.FrameControl != null);
            this.StopCommand = new AsyncRelayCommand(this.StopAction, () => this.IsRunning && this.FrameControl != null);
            this.ToggleStartCommand = new AsyncRelayCommand(this.ToggleStartAction, () => this.FrameControl != null);
            this.SelectOutputFileCommand = new AsyncRelayCommand(this.SelectOutputFileAction);
        }

        public virtual async Task SelectOutputFileAction() {
            if (this.IsRunning) {
                await IoC.MessageDialogs.ShowMessageAsync("Cannot edit file", "Encoding in progress; cannot edit file path");
                return;
            }

            DialogResult<string> result = IoC.FilePicker.ShowSaveFileDialog(Filters.VideoFormatsAndAll, this.FilePath, "Select an output file");
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Value)) {
                this.FilePath = result.Value;
            }
        }

        public override void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo) {
            base.OnAcceptFrame(surface, frameInfo);
            if (!this.IsRunning || !this.mIsOpen) {
                return;
            }

            try {
                this.WriteFrame(surface);
            }
            catch (Exception e) {
                try {
                    this.Close();
                }
                catch (Exception ex) {
                    e.AddSuppressed(ex);
                }
                finally {
                    this.mIsOpen = false;
                    this.IsRunning = false;
                }

                IoC.Dispatcher.InvokeAsync(() => {
                    IoC.MessageDialogs.ShowMessageExAsync("Failed to write frame", "An exception occurred while encoding pixel data", e.GetToString());
                });
            }
        }

        private async Task StartAction() {
            if (this.isRunning)
                return;

            SettingsViewModel settings = IoC.Settings.ActiveSettings;
            this.mParams = new Params() {
                bitrate = 1000000, dst_format = AVPixelFormat.AV_PIX_FMT_D3D11,
                fps = settings.FrameRate,
                width = (uint) settings.Width,
                height = (uint) settings.Height,
                src_format = AVPixelFormat.AV_PIX_FMT_BGRA
            };

            try {
                this.AttemptOpen();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Could not start", "Failed to open FFmpeg stream", e.GetToString());
            }

            this.IsRunning = this.mIsOpen;
        }

        private async Task StopAction() {
            if (!this.isRunning)
                return;

            try {
                this.Close();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Failure while stopping", "Failed to fully stop FFmpeg stream", e.GetToString());
            }
            finally {
                this.mIsOpen = false;
            }

            this.IsRunning = false;
        }

        private unsafe void AttemptOpen() {
            if (this.mIsOpen) {
                try {
                    this.Close();
                }
                catch (Exception e) {
                    throw new Exception("Stream was already open, and the attempt to close it before reopening failed", e);
                }
                finally {
                    this.mIsOpen = false;
                }
            }

            using (ExceptionStack stack = new ExceptionStack("Exception writing video")) {
                try {
                    Context ctx = this.mContext;
                    Params p = this.mParams;
                    this.Open(&ctx, &p);
                }
                catch (Exception e) {
                    stack.Push(e);
                    try {
                        this.Close();
                    }
                    catch (Exception ex) {
                        stack.Push(ex);
                    }
                    finally {
                        this.mIsOpen = false;
                    }
                }
            }
        }

        private async Task ToggleStartAction() {
            if (this.IsRunning) {
                await this.StopAction();
            }
            else {
                await this.StartAction();
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new FFmpegOutputViewModel();
        }

        // /*

        unsafe struct Context {
            public AVFormatContext* format_context;
            public AVStream* stream;
            public AVCodecContext* codec_context;
            public AVFrame* frame;
            public SwsContext* sws_context;
            public AVCodec* codec;
            public uint frame_index;
        };

        unsafe struct Params {
            public uint width;
            public uint height;
            public double fps;
            public uint bitrate;
            public char* preset;
            public uint crf; //0–51
            public AVPixelFormat src_format;
            public AVPixelFormat dst_format;
        };

        private unsafe void Open(Context* ctx, Params* vidparam) {
            ffmpeg.avformat_alloc_output_context2(&ctx->format_context, null, null, this.FilePath);
            if (ctx->format_context == null) {
                throw new Exception("Could not allocate output format");
            }

            ctx->codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            if (ctx->codec == null) {
                throw new Exception("Could not find encoder");
            }

            ctx->stream = ffmpeg.avformat_new_stream(ctx->format_context, null);
            if (ctx->stream == null) {
                throw new Exception("Could not create FFmpeg av-stream");
            }

            ctx->stream->id = (int) (ctx->format_context->nb_streams - 1);
            ctx->codec_context = ffmpeg.avcodec_alloc_context3(ctx->codec);
            if (ctx->codec_context == null) {
                throw new Exception("Could not create codec context");
            }

            ctx->codec_context->codec_id = ctx->format_context->oformat->video_codec;
            ctx->codec_context->bit_rate = vidparam->bitrate;
            ctx->codec_context->width = (int) vidparam->width;
            ctx->codec_context->height = (int) vidparam->height;
            ctx->stream->time_base = ffmpeg.av_d2q(1.0 / vidparam->fps, 120);
            ctx->codec_context->time_base = ctx->stream->time_base;
            ctx->codec_context->framerate = ctx->stream->r_frame_rate;
            ctx->codec_context->pix_fmt = vidparam->dst_format;
            ctx->codec_context->gop_size = 12;
            ctx->codec_context->max_b_frames = 2;

            if ((ctx->format_context->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0)
                ctx->codec_context->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;

            int ret;
            if (vidparam->preset != null) {
                ret = ffmpeg.av_opt_set(ctx->codec_context->priv_data, "preset", new string(vidparam->preset), 0);
                if (ret != 0) {
                    throw new Exception("Could not set preset: " + new string(vidparam->preset) + ". Error code: " + FFmpegError.GetErrorNameAlt(ret));
                }
            }

            {
                ret = ffmpeg.av_opt_set_int(ctx->codec_context->priv_data, "crf", vidparam->crf, 0);
                if (ret != 0) {
                    throw new Exception("Could not set crf to " + vidparam->crf + ". Error code: " + FFmpegError.GetErrorNameAlt(ret));
                }
            }

            ret = ffmpeg.avcodec_open2(ctx->codec_context, ctx->codec, null);
            if (ret != 0) {
                throw new Exception("Could not open codec: " + FFmpegError.GetErrorNameAlt(ret));
            }

            ctx->frame = ffmpeg.av_frame_alloc();
            if (ctx->frame == null) {
                throw new Exception("Could not allocate frame");
            }

            ctx->frame->format = (int) ctx->codec_context->pix_fmt;
            ctx->frame->width = ctx->codec_context->width;
            ctx->frame->height = ctx->codec_context->height;

            ret = ffmpeg.av_frame_get_buffer(ctx->frame, 32);
            if (ret < 0) {
                throw new Exception("Could not allocate frame data: " + FFmpegError.GetErrorNameAlt(ret));
            }

            ret = ffmpeg.avcodec_parameters_from_context(ctx->stream->codecpar, ctx->codec_context);
            if (ret < 0) {
                throw new Exception("Could not copy stream parameters: " + FFmpegError.GetErrorNameAlt(ret));
            }

            ctx->sws_context = ffmpeg.sws_getContext(ctx->codec_context->width, ctx->codec_context->height, vidparam->src_format, // src
                ctx->codec_context->width,
                ctx->codec_context->height, vidparam->dst_format, // dst
                ffmpeg.SWS_BICUBIC, null, null, null
            );
            if (ctx->sws_context == null) {
                throw new Exception("Could not initialize the conversion context");
            }

            ffmpeg.av_dump_format(ctx->format_context, 0, this.FilePath, 1);
            ret = ffmpeg.avio_open(&ctx->format_context->pb, this.FilePath, ffmpeg.AVIO_FLAG_WRITE);
            if (ret != 0) {
                throw new Exception("Could not open file '" + this.FilePath + "'. Error code: " + FFmpegError.GetErrorNameAlt(ret));
            }

            ret = ffmpeg.avformat_write_header(ctx->format_context, null);
            if (ret < 0) {
                Exception exception = new Exception("Could not write header: " + FFmpegError.GetErrorNameAlt(ret));
                int closeRet = ffmpeg.avio_close(ctx->format_context->pb);
                if (closeRet != 0) {
                    exception.AddSuppressed(new Exception("Failed to close file: " + closeRet));
                }

                throw exception;
            }

            ctx->frame_index = 0;
            this.mIsOpen = true;
        }

        public unsafe void WriteFrame(SKSurface surface) {
            byte*[] array = new byte*[1];
            using (SKPixmap pixmap = new SKPixmap()) {
                if (!surface.PeekPixels(pixmap)) {
                    return;
                }

                array[0] = (byte*) pixmap.GetPixels();
                this.WriteFrameData(array);
            }
        }

        public unsafe void WriteFrameData(byte*[] data, int bpp = 4) {
            Context ctx = this.mContext;
            int ret = ffmpeg.av_frame_make_writable(ctx.frame);
            if (ret < 0) {
                throw new Exception("Could not make frame writable");
            }

            int[] in_linesize = {ctx.codec_context->width * bpp};

            ffmpeg.sws_scale(
                ctx.sws_context,
                data,
                in_linesize,
                0,
                ctx.codec_context->height,
                ctx.frame->data,
                ctx.frame->linesize
            );

            ctx.frame->pts = ctx.frame_index++;
            ret = ffmpeg.avcodec_send_frame(ctx.codec_context, ctx.frame);
            if (ret < 0) {
                throw new Exception("Error sending a frame for encoding");
            }

            this.FlushPackets();
        }

        private unsafe void Close() {
            if (this.mIsOpen) {
                ffmpeg.avcodec_send_frame(this.mContext.codec_context, null);
                this.FlushPackets();
                ffmpeg.av_write_trailer(this.mContext.format_context);

                int ret = ffmpeg.avio_close(this.mContext.format_context->pb);
                if (ret != 0) {
                    throw new IOException("Exception closing file. Error code: " + FFmpegError.GetErrorNameAlt(ret));
                }
            }

            if (this.mContext.sws_context != null)
                ffmpeg.sws_freeContext(this.mContext.sws_context);

            fixed (Context* ctx = &this.mContext) {
                if (this.mContext.frame != null)
                    ffmpeg.av_frame_free(&ctx->frame);

                if (this.mContext.codec_context != null)
                    ffmpeg.avcodec_free_context(&ctx->codec_context);
            }

            if (this.mContext.codec_context != null)
                ffmpeg.avcodec_close(this.mContext.codec_context);

            if (this.mContext.format_context != null)
                ffmpeg.avformat_free_context(this.mContext.format_context);

            this.mContext = new Context();
            this.mIsOpen = false;
        }

        private unsafe void FlushPackets() {
            do {
                AVPacket packet = new AVPacket();
                int ret = ffmpeg.avcodec_receive_packet(this.mContext.codec_context, &packet);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF) {
                    break;
                }

                if (ret < 0) {
                    throw new Exception("Exception encoding frame. Error code " + FFmpegError.GetErrorNameAlt(ret));
                }

                ffmpeg.av_packet_rescale_ts(&packet, this.mContext.codec_context->time_base, this.mContext.stream->time_base);
                packet.stream_index = this.mContext.stream->index;

                ret = ffmpeg.av_interleaved_write_frame(this.mContext.format_context, &packet);
                ffmpeg.av_packet_unref(&packet);
                if (ret < 0) {
                    throw new Exception("Exception encoding frame. Error code " + FFmpegError.GetErrorNameAlt(ret));
                }
            } while (true);
        }

        // */
    }
}