#pragma once

#include <QtWidgets/qwidget.h>
#include <QtWidgets/qdockwidget.h>
#include <QtWidgets/qpushbutton.h>

class TestDialog : public QDockWidget {
public:
    explicit TestDialog(QWidget* parent = nullptr);
    ~TestDialog();

private:
    QWidget* m_parent;
    QPushButton* m_button;

private slots:
    void ButtonClicked();
};

