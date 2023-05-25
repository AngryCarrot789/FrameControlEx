#include "TestDialog.h"

#include <QtWidgets/QHBoxLayout>
#include <QtWidgets/QMessageBox>

TestDialog::TestDialog(QWidget* parent) : QDockWidget("My test dialog", parent) {
    m_button = new QPushButton();
    m_parent = parent;

    QWidget* w = new QWidget();
    m_button->setText("Ello!");

    QHBoxLayout* layout = new QHBoxLayout();
    layout->addWidget(m_button);
    w->setLayout(layout);

    setWidget(w);
    setVisible(false);
    setFloating(true);
    resize(300, 300);

    QObject::connect(m_button, SIGNAL(clicked()), SLOT(ButtonClicked));
}

TestDialog::~TestDialog() {

}

void TestDialog::ButtonClicked() {
    QMessageBox::information(this, "Title", "Text");
}
