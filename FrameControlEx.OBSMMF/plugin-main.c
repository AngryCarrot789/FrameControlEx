/*
frame-control-obs-mmf
Copyright (C) 2023 REghZy

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program. If not, see <https://www.gnu.org/licenses/>
*/

/*
    CMake obs plugin template build script doesn't work correctly; referencing QT libraries just breaks everything.
    Instead, I just manually linked the obs-studio and obs-build-dependencies

    If you have a drive with the letter E (e.g. E:\MyFiles), then all you have to do
    is follow the obs auto installation builder thing where you clone the git repo (with the
    recursive option too) into your E drive, and it will create E:\obs-studio for you. Then, you run
    the on the site, you run the first auto installer command in power shell and it should also create
    E:\obs-build-dependencies for you. After that, it should build correctly.

    If you get a compilation error to do with unresolved external symbol "__ms_vsnprintf", then a workaround is
    to open "obs.c" in the libobs project, and append this code after the "#include "obs..h"" code:

    int __cdecl __ms_vsnprintf(char *s, size_t n, const char *format, va_list arg) {
        return 0;
    }

    And then run the script again, and it should build correcrtly, and this project should just work

    If you have obs installed somewhere else, then you can just unload this project and do a search and replace
    for "E:\obs-studio" with your installation (and the same for the dependencies too)

*/

// COMPILING

/*
    You should be able to build normally. Once built, copy "FrameControlEx.OBSMMF.dll" 
    and "FrameControlEx.OBSMMF.pdb" into:
    "C:\Program Files\obs-studio\obs-plugins\64bit"
    or wheverever else your obs is installed, and possibly into 
    the 32 bit folder if you compiled this as 32 bit (why...)

    however if you do link QT libs, it still breaks
*/

#ifndef PLUGINNAME_H
#define PLUGINNAME_H

#define PLUGIN_NAME "FrameControlEx.OBSMMF"
#define PLUGIN_VERSION "1.0.0"

#include <obs-module.h>
#include <obs-frontend-api.h>

OBS_DECLARE_MODULE()
OBS_MODULE_USE_DEFAULT_LOCALE(PLUGIN_NAME, "en-US")

bool obs_module_load(void)
{
    QWidget* wtg;
    obs_frontend_get_main_window();
    // TestWidget* test = new TestWidget(main_window);

    blog(LOG_INFO, "Completely custom plugin loaded! Version: %s", PLUGIN_VERSION);
    return true;
}

void obs_module_unload()
{
    blog(LOG_INFO, "plugin unloaded! :(");
}

#endif // PLUGINNAME_H