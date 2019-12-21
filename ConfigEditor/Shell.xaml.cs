using QXEditorModule.Common;
using QXEditorModule.ViewModels;
using System;
using System.Windows;
using System.Xml;

namespace QueueExchange.ConfigEditor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        TreeEditorsViewModel editorsVM;
        public MainWindow() {
            InitializeComponent();
            this.commandBarView.DocumentLoaded += new EventHandler<DocumentLoadedEventArgs>(commandBarView_DocumentLoaded);

            this.commandBarView.SaveRequested += new EventHandler(commandBarView_SaveRequested);
            this.commandBarView.SaveAsAndExecuteRequested += new EventHandler(commandBarView_SaveAsAndExecuteRequested);
            this.commandBarView.PackageRequested += new EventHandler(commandBarView_PackageRequested);
            this.commandBarView.AboutRequested += new EventHandler(commandBarView_AboutRequested);
            this.commandBarView.SaveAsRequested += new EventHandler<SaveAsEventArgs>(commandBarView_SaveAsRequested);
            editorsVM = new TreeEditorsViewModel();

            this.editorsView.ViewModel = editorsVM;

            this.startUp();
        }

        void startUp() {
            XmlDocument document = new XmlDocument();
            document.LoadXml($"<?xml version=\"1.0\" encoding=\"utf - 8\"?><config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <settings> </settings><pipes><pipe name=\"Name of Pipe\" id=\"{Guid.NewGuid().ToString()}\" numInstances=\"1\" ><input type=\"MSMQ\" id=\"{Guid.NewGuid().ToString()}\" name=\"Description of the Node\" /><output type=\"MSMQ\" id=\"{Guid.NewGuid().ToString()}\" name=\"Description of the Node\" /> </pipe></pipes></config>"); ;
            var xmlTreeViewModel = new TreeEditorViewModel(document, null, "new.xml*");
            editorsVM.Add(xmlTreeViewModel);
        }

        //void commandBarView_SearchRequested(object sender, SearchRequestedEventArgs e)  {          
        //   this.editorsVM.ActiveEditor.FindElementCommand.Execute(e.XPath);
        //}


        void commandBarView_DocumentLoaded(object sender, DocumentLoadedEventArgs e) {
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

        void commandBarView_AboutRequested(object sender, EventArgs e) {
            editorsVM.ActiveEditor.AboutCommand.Execute(null);
        }

        private void editorsView_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}