﻿using Microsoft.Win32;
using QXEditorModule.Common;
using QXEditorModule.GridDefinitions;
using QXEditorModule.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using System.Xml;
using Xceed.Wpf.Toolkit;

namespace QXEditorModule.ViewModels {

    public class TreeEditorViewModel : BaseViewModel {

        private readonly ICommand viewAttributesCommand;
        private readonly ICommand addPipeCommand;
        private readonly ICommand addMonitorCommand;
        private readonly ICommand addNamespaceCommand;
        private readonly ICommand addServiceSettingsCommand;
        private readonly ICommand addInputCommand;
        private readonly ICommand addTypeInputCommand;
        private readonly ICommand addTypeOutputCommand;
        private readonly ICommand addOutputCommand;
        private readonly ICommand addFilterCommand;
        private readonly ICommand addExpressionCommand;
        private readonly ICommand addAltQueueCommand;
        private readonly ICommand addDataFilterCommand;
        private readonly ICommand deleteElementCommand;
        private readonly ICommand saveDocumentCommand;
        private readonly ICommand saveAsDocumentCommand;
        private readonly ICommand executeDocumentCommand;
        private readonly ICommand packageCommand;
        private readonly ICommand aboutCommand;

        private string path;
        private string fileName;
        private SelectedElementViewModel selectedElement = new SelectedElementViewModel(null);

        public MyPropertyGrid MyGrid { get; set; }
        public XmlDocument DataModel { get; private set; }
        public IView View { get; set; }
        public string Path { get { return path; } }
        public string FileName { get { return fileName; } }
        public Func<XmlNodeType, XmlNode> AddXmlNode { get; set; }
        public Action<XmlNode> HighlightNodeInUI { get; set; }

        #region Commands

        public ICommand ViewAttributesCommand { get { return viewAttributesCommand; } }
        public ICommand AddPipeCommand { get { return addPipeCommand; } }
        public ICommand AddMonitorCommand { get { return addMonitorCommand; } }
        public ICommand AddNamespaceCommand { get { return addNamespaceCommand; } }
        public ICommand AddServiceSettingsCommand { get { return addServiceSettingsCommand; } }
        public ICommand AddInputCommand { get { return addInputCommand; } }
        public ICommand AddTypeInputCommand { get { return addTypeInputCommand; } }
        public ICommand AddTypeOutputCommand { get { return addTypeOutputCommand; } }
        public ICommand AddOutputCommand { get { return addOutputCommand; } }
        public ICommand AddFilterCommand { get { return addFilterCommand; } }
        public ICommand AddExpressionCommand { get { return addExpressionCommand; } }
        public ICommand AddAltQueueCommand { get { return addAltQueueCommand; } }
        public ICommand AddDataFilterCommand { get { return addDataFilterCommand; } }
        public ICommand DeleteElementCommand { get { return deleteElementCommand; } }
        public ICommand SaveDocumentCommand { get { return saveDocumentCommand; } }
        public ICommand SaveAsDocumentCommand { get { return saveAsDocumentCommand; } }
        public ICommand SaveAsAndExecuteCommand { get { return executeDocumentCommand; } }
        public ICommand PackageCommand { get { return packageCommand; } }
        public ICommand AboutCommand { get { return aboutCommand; } }

        #endregion
        public TreeEditorViewModel(XmlDocument dataModel, string filePath, string fileName) {
            this.DataModel = dataModel;
            this.path = filePath;
            this.fileName = fileName;
            this.viewAttributesCommand = new RelayCommand<XmlNode>(TreeLeafSelected);
            this.addPipeCommand = new RelayCommand<XmlNodeType>(AddPipe, CanAddPipe);
            this.addInputCommand = new RelayCommand<XmlNodeType>(AddInput, CanAddInOrOut);
            this.addTypeInputCommand = new RelayCommand<String>(AddTypeInput, CanAddTypeInOrOut);
            this.addOutputCommand = new RelayCommand<XmlNodeType>(AddOutput, CanAddInOrOut);
            this.addTypeOutputCommand = new RelayCommand<String>(AddTypeOutput, CanAddTypeInOrOut);
            this.addFilterCommand = new RelayCommand<XmlNodeType>(AddFilter, CanAddFilter);
            this.addExpressionCommand = new RelayCommand<XmlNodeType>(AddExpression, CanAddExpression);
            this.addDataFilterCommand = new RelayCommand<XmlNodeType>(AddDataFilter, CanAddExpression);
            this.addAltQueueCommand = new RelayCommand<XmlNodeType>(AddAltQueue, CanAddAltQueue);
            this.addMonitorCommand = new RelayCommand<XmlNodeType>(AddMonitor, CanAddMonitor);
            this.addNamespaceCommand = new RelayCommand<XmlNodeType>(AddNamespace, CanAddNamespace);
            this.addServiceSettingsCommand = new RelayCommand<XmlNodeType>(AddServiceSetting, CanAddServiceSetting);
            this.saveDocumentCommand = new RelayCommand(p => { Save(); });
            this.saveAsDocumentCommand = new RelayCommand<string>(SaveAs);
            this.executeDocumentCommand = new RelayCommand(p => { ExecuteQX(); });
            this.packageCommand = new RelayCommand(p => { PackageAndSave(); });
            this.aboutCommand = new RelayCommand(p => { AboutQX(); });
            this.deleteElementCommand = new RelayCommand<XmlNode>(p => { DeleteElement(SelectedElement.DataModel); }, p => { return CanDeleteElement(SelectedElement.DataModel); });

        }

        public SelectedElementViewModel SelectedElement {
            get { return selectedElement; }
            private set {
                selectedElement = value;
                try {
                    UpdatePropertiesPanel(selectedElement.DataModel);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
                try {
                    View.HightLightCanvas(selectedElement.DataModel);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }
        public string XMLText {
            get {
                StringBuilder sb = new StringBuilder();
                System.IO.TextWriter tr = new System.IO.StringWriter(sb);
                XmlTextWriter wr = new XmlTextWriter(tr) {
                    Formatting = Formatting.Indented
                };
                DataModel.Save(wr);
                wr.Close();
                return sb.ToString();
            }
        }
        private void TreeLeafSelected(XmlNode newNode) {

            try {
                SelectedElement = new SelectedElementViewModel(newNode);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void UpdatePropertiesPanel(XmlNode selectedItem) {


            if (selectedItem.Name == "input") {
                switch (selectedItem.Attributes["type"].Value) {
                    case "MSMQ":
                        MyGrid = new MSMQIN(selectedItem, this.View);
                        break;
                    case "MQ":
                        MyGrid = new MQIN(selectedItem, this.View);
                        break;
                    case "FILE":
                        MyGrid = new FILEIN(selectedItem, this.View);
                        break;
                    case "KAFKA":
                        MyGrid = new KAFKAIN(selectedItem, this.View);
                        break;
                    case "HTTP":
                        MyGrid = new HTTPIN(selectedItem, this.View);
                        break;
                    case "RABBITDEFEX":
                        MyGrid = new RABBITIN(selectedItem, this.View);
                        break;
                    case "MQTT":
                        MyGrid = new MQTTIN(selectedItem, this.View);
                        break;
                    case "TCPSERVER":
                        MyGrid = new TCPIN(selectedItem, this.View);
                        break;
                    case "TESTSOURCE":
                        MyGrid = new TESTSOURCE(selectedItem, this.View);
                        break;
                }
            } else if (selectedItem.Name == "output" || selectedItem.Name == "altqueue") {
                switch (selectedItem.Attributes["type"].Value) {
                    case "MSMQ":
                        MyGrid = new MSMQOUT(selectedItem, this.View);
                        break;
                    case "MQ":
                        MyGrid = new MQOUT(selectedItem, this.View);
                        break;
                    case "FILE":
                        MyGrid = new FILEOUT(selectedItem, this.View);
                        break;
                    case "KAFKA":
                        MyGrid = new KAFKAOUT(selectedItem, this.View);
                        break;
                    case "REST":
                        MyGrid = new RESTOUT(selectedItem, this.View);
                        break;
                    case "HTTP":
                        MyGrid = new HTTPOUT(selectedItem, this.View);
                        break;
                    case "TCPCLIENT":
                        MyGrid = new TCPOUT(selectedItem, this.View);
                        break;
                    case "RABBITDEFEX":
                        MyGrid = new RABBITOUT(selectedItem, this.View);
                        break;
                    case "MQTT":
                        MyGrid = new MQTTIN(selectedItem, this.View);
                        break;
                    case "SMTP":
                        MyGrid = new SMTPOut(selectedItem, this.View);
                        break;
                    case "FTP":
                        MyGrid = new FTPOut(selectedItem, this.View);
                        break;
                    case "SINK":
                        MyGrid = new SINK(selectedItem, this.View);
                        break;
                }
            } else if (selectedItem.Name == "monitor") {
                MyGrid = new MonitorGrid(selectedItem, this.View);
            } else if (selectedItem.Name == "filter") {
                MyGrid = new Filter(selectedItem, this.View);
            } else if (selectedItem.Name == "pipe") {
                MyGrid = new PIPE(selectedItem, this.View);
            } else if (selectedItem.Name == "namespace") {
                MyGrid = new NameSpaceGrid(selectedItem, this.View);
            } else if (selectedItem.Name == "service") {
                MyGrid = new ServiceSetting(selectedItem, this.View);
            } else if (selectedItem.Name == "and" || selectedItem.Name == "or" || selectedItem.Name == "xor" || selectedItem.Name == "not") {
                MyGrid = new BooleanExpression(selectedItem, this.View);
            } else if (selectedItem.Name == "contains") {
                MyGrid = new ContainsFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "equals") {
                MyGrid = new EqualsFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "matches") {
                MyGrid = new MatchesFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "length") {
                MyGrid = new LengthFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "xpexists") {
                MyGrid = new XPExistsFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "xpmatches") {
                MyGrid = new XPMatchesFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "xpequals") {
                MyGrid = new XPEqualsFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "dateRange") {
                MyGrid = new DateRangeFilter(selectedItem, this.View);
            } else if (selectedItem.Name == "contextContains") {
                MyGrid = new ContextFilter(selectedItem, this.View);
            }

            OnPropertyChanged("MyGrid");
        }


        #region actions


        private void AddPipe(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("pipe");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("name");
            newAttribute.Value = "Descriptive Name of Pipe";
            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString(); ;

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);

            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("numInstances");
            newAttribute2.Value = "1";
            newNode.Attributes.Append(newAttribute2);

            SelectedElement.DataModel.AppendChild(newNode);
            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);

        }

        private bool CanAddPipe(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "pipes") {
                return true;
            } else {
                return false;
            }
        }

        private void AddInput(XmlNodeType newNodeType) {

            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString();

            XmlNode newNode = this.DataModel.CreateElement("input");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "Description of the Node";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);

            if (SelectedElement.DataModel.ChildNodes.Count == 0) {
                SelectedElement.DataModel.AppendChild(newNode);
            } else {
                XmlNode lastNode = null;
                foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                    if (n.Name == "output") {
                        SelectedElement.DataModel.InsertBefore(newNode, n);
                        break;
                    } else {
                        if (lastNode == null) {
                            SelectedElement.DataModel.AppendChild(newNode);
                        } else {
                            lastNode = n;
                        }
                    }
                }
            }

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();
        }

        private XmlNode CreateNodeFromText(string xmlContent) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            XmlNode newNode = doc.DocumentElement;

            return newNode;
        }

        private void AddTypeInput(String type) {

            XmlNode newNode = this.DataModel.CreateElement("input");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = type;
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "Description of the Node";

            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString();

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);

            switch (type) {
                case "AMSMVTUPDATEDMESSAGES":

                    string xml = @"<input type=""MSMQ"" name=""AMS Source"" queue="".\private$\fromams"">
                                    <filter>
                                        <altqueue type = ""MSMQ"" name=""Messages that fail the filter"" queue="".\private$\nonmvtupdated"" />
                                            <xpexists xpath = ""/ams:Envelope/ams:Content/ams:MovementUpdatedNotification"" />
                                    </ filter >
                                </ input > ";
                    newNode = CreateNodeFromText(xml);
                    break;
                default:
                    break;
            }

            if (SelectedElement.DataModel.ChildNodes.Count == 0) {
                SelectedElement.DataModel.AppendChild(newNode);
            } else {
                XmlNode lastNode = null;
                foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                    if (n.Name == "output") {
                        SelectedElement.DataModel.InsertBefore(newNode, n);
                        break;
                    } else {
                        if (lastNode == null) {
                            SelectedElement.DataModel.AppendChild(newNode);
                        } else {
                            lastNode = n;
                        }
                    }
                }
            }



            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);
        }

        private void AddTypeOutput(String type) {

            Guid.NewGuid().ToString();

            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString();

            XmlNode newNode = this.DataModel.CreateElement("output");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = type;
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "Description of the Node";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);

            switch (type) {
                case "REST":
                    XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("maxMessages");
                    newAttribute3.Value = "10";
                    newNode.Attributes.Append(newAttribute3);

                    XmlAttribute newAttribute4 = this.DataModel.CreateAttribute("requestURL");
                    newAttribute4.Value = "http://localhost:8080/qxrestout/";
                    newNode.Attributes.Append(newAttribute4);

                    XmlAttribute newAttribute5 = this.DataModel.CreateAttribute("bufferQueueName");
                    newAttribute5.Value = @".\private$\qxrestbuffer";
                    newNode.Attributes.Append(newAttribute5);

                    break;
                case "RABBITDEFEX":
                    XmlAttribute newAttribute5R = this.DataModel.CreateAttribute("bufferQueueName");
                    newAttribute5R.Value = @".\private$\qxrabbitbuffer";
                    newNode.Attributes.Append(newAttribute5R);

                    break;
                default:
                    break;
            }

            if (SelectedElement.DataModel.ChildNodes.Count == 0) {
                SelectedElement.DataModel.AppendChild(newNode);
            } else {
                XmlNode lastNode = null;
                foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                    if (n.Name == "output") {
                        SelectedElement.DataModel.InsertBefore(newNode, n);
                        break;
                    } else {
                        if (lastNode == null) {
                            SelectedElement.DataModel.AppendChild(newNode);
                        } else {
                            lastNode = n;
                        }
                    }
                }
            }



            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);
        }


        private bool CanAddInOrOut(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "pipe") {
                return true;
            } else {
                return false;
            }
        }


        private bool CanAddTypeInOrOut(string newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "pipe") {
                return true;
            } else {
                return false;
            }
        }


        private void AddOutput(XmlNodeType newNodeType) {

            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString();

            XmlNode newNode = this.DataModel.CreateElement("output");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "Description of the Node";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);

            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

        }
        private void AddFilter(XmlNodeType newNodeType) {

            XmlNode newNode = this.DataModel.CreateElement("filter");

            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                TreeLeafSelected(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();


        }
        private bool CanAddFilter(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null || SelectedElement.DataModel.HasChildNodes) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "input" || SelectedElement.DataModel.Name == "output") {
                return true;
            } else {
                return false;
            }
        }
        private void AddAltQueue(XmlNodeType newNodeType) {

            XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            newAttribute0.Value = Guid.NewGuid().ToString();

            XmlNode newNode = this.DataModel.CreateElement("altqueue");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "Messages that fail the filter";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            newAttribute3.Value = @".\private$\nopassqueue";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);

            if (SelectedElement.DataModel.ChildNodes.Count == 0) {
                SelectedElement.DataModel.AppendChild(newNode);
            } else {
                SelectedElement.DataModel.InsertBefore(newNode, SelectedElement.DataModel.FirstChild);
            }

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

        }
        private bool CanAddAltQueue(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name != "filter") {
                return false;
            }

            foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                if (n.Name == "altqueue") {
                    return false;
                }
            }

            return true;
        }
        private void AddMonitor(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("monitor");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("host");
            newAttribute.Value = "localhost";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("port");
            newAttribute2.Value = "5656";
            //XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            //newAttribute3.Value = @".\private$\qxmonitorqueue";
            //XmlAttribute newAttribute4 = this.DataModel.CreateAttribute("createQueue");
            //newAttribute4.Value = "True";

            //XmlAttribute newAttribute0 = this.DataModel.CreateAttribute("id");
            //newAttribute0.Value = Guid.NewGuid().ToString();

            newNode.Attributes.Append(newAttribute);
            //           newNode.Attributes.Append(newAttribute0);
            newNode.Attributes.Append(newAttribute2);
            //         newNode.Attributes.Append(newAttribute3);
            //         newNode.Attributes.Append(newAttribute4);

            SelectedElement.DataModel.InsertBefore(newNode, SelectedElement.DataModel.FirstChild);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);

        }
        private bool CanAddMonitor(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name != "settings") {
                return false;
            }

            foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                if (n.Name == "monitor") {
                    return false;
                }
            }

            return true;
        }
        private void AddNamespace(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("namespace");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("prefix");
            newAttribute.Value = "";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("uri");
            newAttribute2.Value = "";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);

            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);

        }
        private bool CanAddNamespace(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "settings") {
                return true;
            } else {
                return false;
            }
        }


        private void AddServiceSetting(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("service");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("serviceName");
            newAttribute.Value = "Queue Exchange Service";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("serviceDisplayName");
            newAttribute2.Value = "Queue Exchange Service";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("serviceDescription");
            newAttribute3.Value = "A instance of QueueExchange for connecting inputs to outputs";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);

            if (!SelectedElement.DataModel.HasChildNodes) {
                SelectedElement.DataModel.AppendChild(newNode);
            } else {
                if (SelectedElement.DataModel.FirstChild.Name == "monitor") {
                    SelectedElement.DataModel.InsertAfter(newNode, SelectedElement.DataModel.FirstChild);
                } else {
                    SelectedElement.DataModel.InsertBefore(newNode, SelectedElement.DataModel.FirstChild);
                }
            }


            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

            View.HightLightCanvas(newNode);

        }
        private bool CanAddServiceSetting(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name != "settings") {
                return false;
            }

            foreach (XmlNode n in SelectedElement.DataModel.ChildNodes) {
                if (n.Name == "service") {
                    return false;
                }
            }

            return true;
        }
        private void AddDataFilter(XmlNodeType newNodeType) {

            XmlNode newNode = this.DataModel.CreateElement("contains");
            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

        }
        private void AddExpression(XmlNodeType newNodeType) {

            XmlNode newNode = this.DataModel.CreateElement("and");
            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();

        }
        private bool CanAddExpression(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "filter") {
                if (!SelectedElement.DataModel.HasChildNodes) {
                    return true;
                }
                if (SelectedElement.DataModel.ChildNodes.Count == 1 && SelectedElement.DataModel.ChildNodes.Item(0).Name == "altqueue") {
                    return true;
                }

                return false;
            }
            if (SelectedElement.DataModel.Name == "and" || SelectedElement.DataModel.Name == "or" || SelectedElement.DataModel.Name == "xor") {
                return true;
            }
            if (SelectedElement.DataModel.Name == "not") {
                if (!SelectedElement.DataModel.HasChildNodes) {
                    return true;
                }
            }

            return false;
        }

        private void DeleteElement(XmlNode currentNode) {
            currentNode.ParentNode.RemoveChild(currentNode);
            OnPropertyChanged("XMLText");
            View.DrawQXConfig();
            MyGrid = null;
            OnPropertyChanged("MyGrid");
        }

        private bool CanDeleteElement(XmlNode currentNode) {

            if (currentNode == null) {
                return false;
            }
            if (currentNode.ParentNode == null) {
                return false;
            }
            if (SelectedElement.DataModel.Name == "pipes" || SelectedElement.DataModel.Name == "settings") {
                return false;
            } else if (currentNode.NodeType == XmlNodeType.Text || currentNode.NodeType == XmlNodeType.Element) {
                return true;
            }
            return false;
        }

        internal void ChangeFilterType(string value) {

            //"Data Contains Value", "Data Matches Regex.", "Data Minimum Length", "XPath Exists","XPath Equals", "Xpath Date Within Offset"

            string name = "contains";
            if (value == "Data Contains Value") name = "contains";
            if (value == "Data Equals Value") name = "equals";
            if (value == "Data Matches Regex.") name = "matches";
            if (value == "Data Minimum Length") name = "length";
            if (value == "XPath Exists") name = "xpexists";
            if (value == "XPath Equals") name = "xpequals";
            if (value == "XPath Matches") name = "xpmatches";
            if (value == "XPath Date Within Offset") name = "dateRange";
            if (value == "Context Contains") name = "contextContains";

            XmlNode newNode = this.DataModel.CreateElement(name);
            SelectedElement.DataModel.ParentNode.InsertAfter(newNode, SelectedElement.DataModel);

            SelectedElement.DataModel.ParentNode.RemoveChild(SelectedElement.DataModel);

            ViewAttributesCommand.Execute(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();
        }

        internal bool CanChangeElementType(string value) {
            if (value == "not" && SelectedElement.DataModel.ChildNodes.Count > 1) {
                MessageBox.Show("Cannot change to 'not' beacause a 'not' can only have one direct child", "QueueExchange Configuration");
                MyGrid = new BooleanExpression(SelectedElement.DataModel, this.View);
                OnPropertyChanged("MyGrid");
                return false;
            } else {
                return true;
            }
        }

        internal void ChangeElementType(string value) {

            if (value == "not" && SelectedElement.DataModel.ChildNodes.Count > 1) {
                MessageBox.Show("Cannot change to 'not' beacause a 'not' can only have one direct child", "QueueExchange Configuration");
                OnPropertyChanged("XMLText");
                View.DrawQXConfig();
                return;
            }

            XmlNode newNode = this.DataModel.CreateElement(value);
            SelectedElement.DataModel.ParentNode.InsertAfter(newNode, SelectedElement.DataModel);

            foreach (XmlNode child in SelectedElement.DataModel.ChildNodes) {
                XmlNode move = child.CloneNode(true);
                newNode.AppendChild(move);
            }

            SelectedElement.DataModel.ParentNode.RemoveChild(SelectedElement.DataModel);
            ViewAttributesCommand.Execute(newNode);

            OnPropertyChanged("XMLText");
            View.DrawQXConfig();
        }

        private void Save() {


            if (Path == null) {
                SaveFileDialog dialog = new SaveFileDialog {
                    Filter = "XML Files (*.xml)|*.xml"
                };
                if (dialog.ShowDialog() == true) {
                    using (TextWriter sw = new StreamWriter(dialog.FileName, false, Encoding.UTF8)) {
                        this.DataModel.Save(sw);

                        this.path = dialog.FileName;
                        string[] f = dialog.FileName.Split('\\');
                        this.fileName = f[f.Length - 1];

                        OnPropertyChanged("FileName");
                        OnPropertyChanged("Path");
                    }
                    //              UnloadEditor();
                    return;
                } else {
                    return;
                }
            }

            using (TextWriter sw = new StreamWriter(Path, false, Encoding.UTF8)) {
                this.DataModel.Save(sw);
            }

        }

        private void SaveAs(string path) {
            XmlDocument newDoc = this.DataModel.CloneNode(true) as XmlDocument;
            using (TextWriter sw = new StreamWriter(path, false, Encoding.UTF8)) {
                newDoc.Save(sw);
            }

            this.path = path;
            string[] f = path.Split('\\');
            this.fileName = f[f.Length - 1];

            OnPropertyChanged("FileName");
            OnPropertyChanged("Path");
        }
        private void ExecuteQX() {
            XmlDocument newDoc = this.DataModel.CloneNode(true) as XmlDocument;
            using (TextWriter sw = new StreamWriter(@"./Executable/ExchangeConfig.xml", false, Encoding.UTF8)) {
                newDoc.Save(sw);
            }

            string serviceName;
            string serviceDisplayName;
            string serviceDescription;

            try {

                // If the service settings are set, then write them into the QX.exe.config file
                XmlNode serviceSettings = this.DataModel.SelectSingleNode("//service");

                try {
                    serviceName = serviceSettings.Attributes["serviceName"].Value;
                } catch (Exception) {
                    serviceName = "Queue Exchange Service";
                }

                try {
                    serviceDisplayName = serviceSettings.Attributes["serviceDisplayName"].Value;
                } catch (Exception) {
                    serviceDisplayName = "Queue Exchange Service";
                }
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string versionInfo = String.Format("Last Build: - version {0}", version);

                serviceDisplayName = serviceDisplayName + "  " + versionInfo;

                try {
                    serviceDescription = serviceSettings.Attributes["serviceDescription"].Value;
                } catch (Exception) {
                    serviceDescription = "Connects input nodes with output nodes";
                }


                XmlDocument doc = new XmlDocument();
                doc.Load("./Executable/QX.exe.config");

                doc.SelectSingleNode("//add[@key='ServiceName']").Attributes["value"].Value = serviceName;
                doc.SelectSingleNode("//add[@key='ServiceDisplayName']").Attributes["value"].Value = serviceDisplayName;
                doc.SelectSingleNode("//add[@key='ServiceDescription']").Attributes["value"].Value = serviceDescription;

                using (TextWriter sw = new StreamWriter(@"./Executable/QX.exe.config", false, Encoding.UTF8)) {
                    doc.Save(sw);
                }

            } catch (Exception e) {
                Console.WriteLine($"Setting Service Parameters Not Found in ExchangeConfig.xml - using defaults. Error Message  {e.Message}");
            }


            ProcessStartInfo startInfo = new ProcessStartInfo {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = @"./Executable/QX.exe"
            };

            System.Diagnostics.Process.Start(startInfo);
        }

        private void PackageAndSave() {

            string serviceName = "Queue Exchange Service";
            string serviceDisplayName = "Queue Exchange Service";
            string serviceDescription = "Connects input nodes with output nodes";


            SaveFileDialog dialog = new SaveFileDialog {
                Filter = "Zip Files (*.zip)|*.zip"
            };
            if (dialog.ShowDialog() == true) {

                try {

                    // If the service settings are set, then write them into the QX.exe.config file
                    XmlNode serviceSettings = this.DataModel.SelectSingleNode("//service");

                    try {
                        serviceName = serviceSettings.Attributes["serviceName"].Value;
                    } catch (Exception) {
                        serviceName = "Queue Exchange Service";
                    }

                    try {
                        serviceDisplayName = serviceSettings.Attributes["serviceDisplayName"].Value;
                    } catch (Exception) {
                        serviceDisplayName = "Queue Exchange Service";
                    }
                    string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string versionInfo = String.Format("Last Build: - version {0}", version);

                    serviceDisplayName = serviceDisplayName + "  " + versionInfo;

                    try {
                        serviceDescription = serviceSettings.Attributes["serviceDescription"].Value;
                    } catch (Exception) {
                        serviceDescription = "Connects input nodes with output nodes";
                    }


                    XmlDocument doc = new XmlDocument();
                    doc.Load("./Executable/QX.exe.config");

                    doc.SelectSingleNode("//add[@key='ServiceName']").Attributes["value"].Value = serviceName;
                    doc.SelectSingleNode("//add[@key='ServiceDisplayName']").Attributes["value"].Value = serviceDisplayName;
                    doc.SelectSingleNode("//add[@key='ServiceDescription']").Attributes["value"].Value = serviceDescription;

                    using (TextWriter sw = new StreamWriter(@"./Executable/QX.exe.config", false, Encoding.UTF8)) {
                        doc.Save(sw);
                    }

                } catch (Exception e) {
                    Console.WriteLine($"Setting Service Parameters Not Found in ExchangeConfig.xml - using defaults. Error Message  {e.Message}");
                }


                XmlDocument newDoc = this.DataModel.CloneNode(true) as XmlDocument;
                using (TextWriter sw = new StreamWriter(@"./Executable/ExchangeConfig.xml", false, Encoding.UTF8)) {
                    newDoc.Save(sw);
                }

                if (File.Exists(dialog.FileName)) {
                    File.Delete(dialog.FileName);
                }

                QXPackageMsg dlg = new QXPackageMsg(serviceName, serviceDisplayName, serviceDescription, dialog.FileName);
                dlg.ShowDialog();

                ZipFile.CreateFromDirectory(@"./Executable/", dialog.FileName);

            }
        }

        private void AboutQX() {
            QXAbout dlg = new QXAbout();
            dlg.ShowDialog();
        }

        #endregion
    }
}
