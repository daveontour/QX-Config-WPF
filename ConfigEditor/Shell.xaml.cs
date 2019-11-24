using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views;
using ConfigEditor;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.ViewModels;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.ConfigEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TreeEditorsViewModel editorsVM;
        public MainWindow()
        {
            InitializeComponent();     
            this.commandBarView.DocumentLoaded += new EventHandler<DocumentLoadedEventArgs>(commandBarView_DocumentLoaded);
           
            this.commandBarView.SaveRequested += new EventHandler(commandBarView_SaveRequested);
            this.commandBarView.SaveAsAndExecuteRequested += new EventHandler(commandBarView_SaveAsAndExecuteRequested);
            this.commandBarView.PackageRequested += new EventHandler(commandBarView_PackageRequested);
            this.commandBarView.SaveAsRequested += new EventHandler<SaveAsEventArgs>(commandBarView_SaveAsRequested);
            editorsVM = new TreeEditorsViewModel();
           
            this.editorsView.ViewModel = editorsVM;

            this.startUp();
        }

        void startUp() {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<?xml version=\"1.0\" encoding=\"utf - 8\"?><config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <settings> </settings><pipes></pipes></config>"); ;
            var xmlTreeViewModel = new TreeEditorViewModel(document, null, "new.xml*");
            editorsVM.Add(xmlTreeViewModel);
        }

        void commandBarView_SearchRequested(object sender, SearchRequestedEventArgs e)  {          
           this.editorsVM.ActiveEditor.FindElementCommand.Execute(e.XPath);
        }

      
        void commandBarView_DocumentLoaded(object sender, DocumentLoadedEventArgs e){
            var xmlTreeViewModel = new TreeEditorViewModel(e.Document, e.Path, e.FileName);            
            editorsVM.Add(xmlTreeViewModel);            
        }

        void commandBarView_SaveAsRequested(object sender, SaveAsEventArgs e) {
            editorsVM.ActiveEditor.SaveAsDocumentCommand.Execute(e.Path);
        }

        void commandBarView_SaveRequested(object sender, EventArgs e) {
            editorsVM.ActiveEditor.SaveDocumentCommand.Execute(null);
        }

        void commandBarView_SaveAsAndExecuteRequested(object sender, EventArgs e) {
            editorsVM.ActiveEditor.SaveAsAndExecuteCommand.Execute(null);
        }

        void commandBarView_PackageRequested(object sender, EventArgs e) {
            editorsVM.ActiveEditor.PackageCommand.Execute(null);
        }
    }
}