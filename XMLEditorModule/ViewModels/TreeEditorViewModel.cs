﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WXE.Internal.Tools.ConfigEditor.Common;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.ViewModels {
    public class TreeEditorViewModel : BaseViewModel {
        public TreeEditorViewModel(XmlDocument dataModel, string filePath, string fileName) {
            this.DataModel = dataModel;
            this.path = filePath;
            this.fileName = fileName;
            this.findElementCommand = new RelayCommand<string>(HighlightElement, CanHighlightElement);
            this.viewAttributesCommand = new RelayCommand<XmlNode>(ViewAttributes);
            this.copyElementCommand = new RelayCommand<XmlNode>(p => { Copy(SelectedElement.DataModel); }, p => { return CanCopy(SelectedElement.DataModel); });
            this.pasteElementCommand = new RelayCommand((p) => { Paste(SelectedElement.DataModel); }, (p) => { return CanPaste(SelectedElement.DataModel); });
            this.addElementCommand = new RelayCommand<XmlNodeType>(AddElement, CanAddElement);
            this.addPipeCommand = new RelayCommand<XmlNodeType>(AddPipe, CanAddPipe);
            this.addInputCommand = new RelayCommand<XmlNodeType>(AddInput, CanAddInput);
            this.addOutputCommand = new RelayCommand<XmlNodeType>(AddOutput, CanAddOutput);
            this.addFilterCommand = new RelayCommand<XmlNodeType>(AddFilter, CanAddFilter);
            this.addExpressionCommand = new RelayCommand<XmlNodeType>(AddExpression, CanAddExpression);
            this.addMonitorCommand = new RelayCommand<XmlNodeType>(AddMonitor, CanAddMonitor);
            this.addLoggerCommand = new RelayCommand<XmlNodeType>(AddLogger, CanAddLogger);
            this.addNamespaceCommand = new RelayCommand<XmlNodeType>(AddNamespace, CanAddNamespace);
            this.saveDocumentCommand = new RelayCommand(p => { Save(); });
            this.saveAsDocumentCommand = new RelayCommand<string>(SaveAs);
            this.deleteElementCommand = new RelayCommand<XmlNode>(p => { DeleteElement(SelectedElement.DataModel); }, p => { return CanDeleteElement(SelectedElement.DataModel); });
            this.unloadDocumentCommand = new RelayCommand(UnloadDocument);
        }
        public XmlDocument DataModel { get; private set; }

        public static XmlNode CopiedElement;

        private string path;

        public string Path {
            get { return path; }

        }

        private string fileName;

        public string FileName {
            get { return fileName; }
        }


        private SelectedElementViewModel selectedElement = new SelectedElementViewModel(null);

        public SelectedElementViewModel SelectedElement {
            get { return selectedElement; }
            private set {
                selectedElement = value;
                OnPropertyChanged("SelectedElement");
                SelectedNodeXpath = GetXPathToNode(SelectedElement.DataModel);
            }
        }

        public string XMLText {
            get {
                StringBuilder sb = new StringBuilder();
                System.IO.TextWriter tr = new System.IO.StringWriter(sb);
                XmlTextWriter wr = new XmlTextWriter(tr);
                wr.Formatting = Formatting.Indented;
                DataModel.Save(wr);
                wr.Close();
                return sb.ToString();
            }

        }

        private string selectedNodeXpath = string.Empty;

        public string SelectedNodeXpath {
            get { return selectedNodeXpath; }
            set {
                selectedNodeXpath = value;
                OnPropertyChanged("SelectedNodeXpath");
            }
        }



        /// <summary>
        /// To get the value from UI
        /// </summary>
        public Func<XmlNodeType, XmlNode> AddXmlNode { get; set; }


        private string prevXPath;

        public Action<XmlNode> HighlightNodeInUI { get; set; }

        #region Commands

        private ICommand findElementCommand;

        public ICommand FindElementCommand {
            get { return findElementCommand; }
        }

        private ICommand viewAttributesCommand;

        public ICommand ViewAttributesCommand {
            get { return viewAttributesCommand; }
        }



        private ICommand addElementCommand;

        public ICommand AddElementCommand {
            get { return addElementCommand; }
        }

        private ICommand addPipeCommand;

        public ICommand AddPipeCommand {
            get { return addPipeCommand; }
        }

        private ICommand addMonitorCommand;

        public ICommand AddMonitorCommand {
            get { return addMonitorCommand; }
        }

        private ICommand addLoggerCommand;

        public ICommand AddLoggerCommand {
            get { return addLoggerCommand; }
        }

        private ICommand addNamespaceCommand;

        public ICommand AddNamespaceCommand {
            get { return addNamespaceCommand; }
        }

        private ICommand addInputCommand;

        public ICommand AddInputCommand {
            get { return addInputCommand; }
        }

        private ICommand addOutputCommand;

        public ICommand AddOutputCommand {
            get { return addOutputCommand; }
        }

        private ICommand addFilterCommand;

        public ICommand AddFilterCommand {
            get { return addFilterCommand; }
        }

        private ICommand addExpressionCommand;

        public ICommand AddExpressionCommand {
            get { return addExpressionCommand; }
        }
        private ICommand copyElementCommand;

        public ICommand CopyElementCommand {
            get { return copyElementCommand; }
        }

        private ICommand pasteElementCommand;

        public ICommand PasteElementCommand {
            get { return pasteElementCommand; }
        }


        private ICommand deleteElementCommand;

        public ICommand DeleteElementCommand {
            get { return deleteElementCommand; }
        }


        private ICommand unloadDocumentCommand;

        public ICommand UnloadDocumentCommand {
            get { return unloadDocumentCommand; }
        }

        private ICommand saveDocumentCommand;

        public ICommand SaveDocumentCommand {
            get { return saveDocumentCommand; }
        }

        private ICommand saveAsDocumentCommand;

        public ICommand SaveAsDocumentCommand {
            get { return saveAsDocumentCommand; }
        }




        private bool canMoveNext;

        public bool CanMoveNext {
            get { return canMoveNext; }
            private set {
                canMoveNext = value;

                if (!value) {
                    prevXPath = null;
                }
            }
        }

        IEnumerator<XmlNode> enumerator;
        private void HighlightElement(string xPath) {
            //
            if (xPath != prevXPath) {
                try {
                    XmlNodeList nodeList = null;

                    if (DataModel.DocumentElement.Attributes["xmlns:xsi"] != null) {

                        string xmlns = DataModel.DocumentElement.Attributes["xmlns:xsi"].Value;
                        xPath = DataModel.DocumentElement.Name + xPath + "/*";
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(DataModel.NameTable);

                        nsmgr.AddNamespace(DataModel.DocumentElement.Name, xmlns);
                        xmlns = DataModel.DocumentElement.Attributes["xmlns:xsd"].Value;
                        nsmgr.AddNamespace(DataModel.DocumentElement.Name, xmlns);
                        nodeList = DataModel.SelectNodes(xPath, nsmgr);
                    } else {
                        nodeList = DataModel.SelectNodes(xPath);
                    }


                    if (nodeList.Count > 0) {
                        CanMoveNext = true;
                        prevXPath = xPath;
                    } else {
                        CanMoveNext = false;

                    }
                    enumerator = GetNextNode(nodeList);
                    CanMoveNext = enumerator.MoveNext();
                } catch {
                    CanMoveNext = false;
                }
            }
            if (enumerator != null) {
                XmlNode xmlNode = enumerator.Current;
                CanMoveNext = enumerator.MoveNext();
                if (xmlNode == null) {
                    CanMoveNext = false;
                }
                if (HighlightNodeInUI != null) {
                    HighlightNodeInUI(xmlNode);
                }
            } else {
                CanMoveNext = false;
            }


        }

        IEnumerator<XmlNode> GetNextNode(XmlNodeList nodeList) {
            if (nodeList == null) {
                yield return null;
            }
            foreach (XmlNode xmlNode in nodeList) {

                yield return xmlNode;
            }
        }


        private bool CanHighlightElement(string xPath) {
            return (!string.IsNullOrEmpty(xPath));
        }


        private void ViewAttributes(XmlNode newNode) {

            //TODO: Populate SelectedElementViewModel.
            if (SelectedElement != null) {
                SelectedElement.AddXmlNode = null;
            }
            SelectedElement = new SelectedElementViewModel(newNode);
            SelectedElement.AddXmlNode = this.AddXmlNode;
        }


        private void Copy(XmlNode node) {
            TreeEditorViewModel.CopiedElement = node.CloneNode(true);

        }

        private bool CanCopy(XmlNode node) {
            return node != null;
        }

        private void Paste(XmlNode parentNode) {
            if (parentNode.NodeType == XmlNodeType.Element) {
                XmlNode xmlNode = null;
                if (parentNode.OwnerDocument != TreeEditorViewModel.CopiedElement.OwnerDocument) {
                    xmlNode = parentNode.OwnerDocument.ImportNode(TreeEditorViewModel.CopiedElement, true);
                } else {
                    xmlNode = TreeEditorViewModel.CopiedElement;
                }
                parentNode.InsertAfter(xmlNode, parentNode.LastChild);
                Copy(TreeEditorViewModel.CopiedElement);

            }
        }

        private bool CanPaste(XmlNode parentNode) {
            bool canPaste = false;
            if (parentNode != null && parentNode.FirstChild != null && parentNode.FirstChild == parentNode.LastChild && parentNode.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (parentNode != null && parentNode.NodeType == XmlNodeType.Element) {
                if (TreeEditorViewModel.CopiedElement != null) {
                    canPaste = true;
                }

            }
            return canPaste;
        }

        private void AddPipe(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("pipe");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("name");
            newAttribute.Value = "Descriptive Name of Pipe";
            newNode.Attributes.Append(newAttribute);

            SelectedElement.DataModel.AppendChild(newNode);
            OnPropertyChanged("XMLText");

        }

        private bool CanAddPipe(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "pipes") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }

        private void AddInput(XmlNodeType newNodeType) {

            XmlNode newNode = this.DataModel.CreateElement("input");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "descriptive queue name";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            newAttribute3.Value = @".\private$\QUEUENAME";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);


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

        }

        private bool CanAddInput(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "pipe") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }

        private void AddOutput(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("output");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "descriptive queue name";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            newAttribute3.Value = @".\private$\QUEUENAME";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);

            SelectedElement.DataModel.AppendChild(newNode);

            OnPropertyChanged("XMLText");

        }

        private bool CanAddOutput(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "pipe") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }

        private void AddFilter(XmlNodeType newNodeType) {

            XmlNode newNode = this.DataModel.CreateElement("filter");


            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");


        }

        private bool CanAddFilter(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "input" || SelectedElement.DataModel.Name == "output") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }

        private void AddMonitor(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("monitor");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "descriptive queue name";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            newAttribute3.Value = @".\private$\QUEUENAME";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);

            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");

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
        private void AddLogger(XmlNodeType newNodeType) {
            XmlNode newNode = this.DataModel.CreateElement("logger");
            XmlAttribute newAttribute = this.DataModel.CreateAttribute("type");
            newAttribute.Value = "MSMQ";
            XmlAttribute newAttribute2 = this.DataModel.CreateAttribute("name");
            newAttribute2.Value = "descriptive queue name";
            XmlAttribute newAttribute3 = this.DataModel.CreateAttribute("queue");
            newAttribute3.Value = @".\private$\QUEUENAME";

            newNode.Attributes.Append(newAttribute);
            newNode.Attributes.Append(newAttribute2);
            newNode.Attributes.Append(newAttribute3);

            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");

        }

        private bool CanAddLogger(XmlNodeType newNodeType) {

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
                if (n.Name == "logger") {
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


            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");

        }

        private bool CanAddNamespace(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "settings") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }

        private void AddExpression(XmlNodeType newNodeType) {
            XmlNode newNode = AddXmlNode(newNodeType);

            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

            OnPropertyChanged("XMLText");

        }

        private bool CanAddExpression(XmlNodeType newNodeType) {

            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }

            if (SelectedElement.DataModel.Name == "filter") {
                return true;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return false;
            }
        }
        private void AddElement(XmlNodeType newNodeType) {
            XmlNode newNode = AddXmlNode(newNodeType);

            if (newNode == null)
                return;
            if (newNode.NodeType == XmlNodeType.Attribute) {
                SelectedElement.DataModel.Attributes.Append(newNode as XmlAttribute);
                ViewAttributes(SelectedElement.DataModel);
            } else {
                SelectedElement.DataModel.AppendChild(newNode);
            }

        }

        private bool CanAddElement(XmlNodeType newNodeType) {
            if (SelectedElement == null || SelectedElement.DataModel == null) {
                return false;
            }
            if (SelectedElement.DataModel.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (SelectedElement.DataModel.FirstChild != null && SelectedElement.DataModel.FirstChild == SelectedElement.DataModel.LastChild && SelectedElement.DataModel.FirstChild.NodeType != XmlNodeType.Element) {
                return false;
            }
            if (AddXmlNode == null) {
                return false;
            } else {
                return true;
            }
        }

        private void DeleteElement(XmlNode currentNode) {
            //TODO:

            currentNode.ParentNode.RemoveChild(currentNode);
            OnPropertyChanged("XMLText");

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

        private void Save() {
            this.DataModel.Save(Path);
        }

        private void SaveAs(string path) {
            XmlDocument newDoc = this.DataModel.CloneNode(true) as XmlDocument;
            newDoc.Save(path);
        }
        private void UnloadDocument(object param) {
            this.DataModel.Save(Path);
        }

        public void UnloadEditor() {
            UnloadDocument(null);
            SelectedNodeXpath = string.Empty;
        }


        #endregion


        #region Getting Xpath

        private static string GetXPathToNode(XmlNode node) {
            if (node == null)
                return string.Empty;
            if (node.NodeType == XmlNodeType.Attribute) {
                // attributes have an OwnerElement, not a ParentNode; also they have
                // to be matched by name, not found by position
                return String.Format(
                    "{0}/@{1}",
                    GetXPathToNode(((XmlAttribute)node).OwnerElement),
                    node.Name
                    );
            }
            if (node.ParentNode == null) {
                // the only node with no parent is the root node, which has no path
                return "";
            }
            //get the index
            int iIndex = 1;
            XmlNode xnIndex = node;
            while (xnIndex.PreviousSibling != null) { iIndex++; xnIndex = xnIndex.PreviousSibling; }
            // the path to a node is the path to its parent, plus "/node()[n]", where 
            // n is its position among its siblings.
            return String.Format(
                "{0}/{1}",
                GetXPathToNode(node.ParentNode),
                node.Name
                );
        }
        #endregion
    }
}
