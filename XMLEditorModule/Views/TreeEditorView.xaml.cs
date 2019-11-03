using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.ViewModels;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using System.IO;

using Path = System.Windows.Shapes.Path;
using System.Windows.Markup;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class TreeEditorView : UserControl, INotifyPropertyChanged, IView {

        private ContextMenuProvider contextMenuProvider;
        private const int arrowHeadWidth = 5;
        private const int arrowHeadLength = 12;
        public event PropertyChangedEventHandler PropertyChanged;
        public TreeEditorViewModel viewModel;
        public Dictionary<XmlNode, Canvas> nodeToCanvas = new Dictionary<XmlNode, Canvas>();
        public Dictionary<Canvas, XmlNode> canvasToNode = new Dictionary<Canvas, XmlNode>();
        public Canvas selectedCanvas;

        protected void OnPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public TreeEditorView() {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(TreeEditorView_DataContextChanged);
            contextMenuProvider = new ContextMenuProvider();
            xmlTreeView.ContextMenu = new ContextMenu();

        }

        public Canvas SelectedCanvas {
            get { return this.selectedCanvas; }
            set {
                if (this.selectedCanvas != null) {
                    SolidColorBrush brush = new SolidColorBrush();
                    brush.Color = Colors.Transparent;
                    this.selectedCanvas.Background = brush;
                }
                this.selectedCanvas = value;
            }
        }

        private Canvas GetInputCanvas(double imHeight, string title) {

            double fs = 9;
            double h = imHeight - fs - 2;
            double w = h * 0.8563;
            double tY = h;
            double tX = 2;

            Canvas imcanvas = GetResourceCopy<Canvas>("bat");
            imcanvas.Height = h;
            imcanvas.Width = w;

            Path path = LogicalTreeHelper.FindLogicalNode(imcanvas, "path") as Path;
            path.Height = h;
            path.Width = w;

            TextBlock txt = new TextBlock();
            txt.Text = title;
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            txt.FontSize = fs;
            txt.SetValue(Canvas.LeftProperty, tX);
            txt.SetValue(Canvas.TopProperty, tY);

            Canvas can = new Canvas();
            can.Height = imHeight;
            can.Width = w;
            can.Children.Add(imcanvas);
            can.Children.Add(txt);

            return can;
        }

        private Canvas GetOutputCanvas(double imHeight, string title) {

            double fs = 9;
            double h = imHeight - fs - 2;
            double w = h;
            double tY = h;
            double tX = 2;

            Canvas imcanvas = GetResourceCopy<Canvas>("dup");
            imcanvas.Height = h;
            imcanvas.Width = w;

            Path path = LogicalTreeHelper.FindLogicalNode(imcanvas, "path2") as Path;
            path.Height = h;
            path.Width = w;

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, 128, 128, 128);
            path.Fill = mySolidColorBrush;

            TextBlock txt = new TextBlock();
            txt.Text = title;
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            txt.FontSize = fs;
            txt.SetValue(Canvas.LeftProperty, tX);
            txt.SetValue(Canvas.TopProperty, tY);

            Canvas can = new Canvas();
            can.Height = imHeight;
            can.Width = w;
            can.Children.Add(imcanvas);
            can.Children.Add(txt);

            return can;
        }

        public Canvas GetMonitorCanvas() {

            Canvas can = new Canvas();
            Canvas monitorcanvas = GetResourceCopy<Canvas>("monitor");

            can.Children.Add(monitorcanvas);

            TextBlock txt = new TextBlock() {Text = "Monitor", FontSize = 8};
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)monitorcanvas.Height+2);

            can.Children.Add(txt);

            return can;
        }

        public Canvas GetLoggerCanvas() {

            Canvas can = new Canvas();
            Canvas monitorcanvas = GetResourceCopy<Canvas>("logger");

            can.Children.Add(monitorcanvas);

            TextBlock txt = new TextBlock() { Text = "Logger", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)monitorcanvas.Height + 2);

            can.Children.Add(txt);

            return can;
        }

        public Canvas GetNSCanvas() {

            Canvas can = new Canvas();
            Canvas nscanvas = GetResourceCopy<Canvas>("namespace");

            can.Children.Add(nscanvas);

            TextBlock txt = new TextBlock() { Text = "Namespace", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)32);

            can.Children.Add(txt);

            return can;
        }
        public void DrawConfig(XmlDocument xmlDoc) {

            this.panel.Children.Clear();
            this.settingspanel.Children.Clear();
            this.canvasToNode.Clear();
            this.nodeToCanvas.Clear();

           

            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            SolidColorBrush whiteBrush = new SolidColorBrush();
            whiteBrush.Color = Colors.White;

            SolidColorBrush bluishBrush = new SolidColorBrush();
            bluishBrush.Color = Color.FromArgb(255, 135, 206, 250);

            SolidColorBrush transBrush = new SolidColorBrush();
            transBrush.Color = Colors.Transparent;

            XmlNodeList monitors = xmlDoc.SelectNodes("//monitor");
            foreach (XmlNode monitor in monitors) {
                Canvas monitorCanvas = GetMonitorCanvas();
                monitorCanvas.PreviewMouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(monitor, monitorCanvas);
                this.canvasToNode.Add(monitorCanvas, monitor);
                settingspanel.Children.Add(monitorCanvas);
            }

            XmlNodeList loggers = xmlDoc.SelectNodes("//logger");
            foreach (XmlNode logger in loggers) {
                Canvas logCanvas = GetLoggerCanvas();
                logCanvas.PreviewMouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(logger, logCanvas);
                this.canvasToNode.Add(logCanvas, logger);
                settingspanel.Children.Add(logCanvas);
            }

 
            XmlNodeList nss = xmlDoc.SelectNodes("//namespace");
            foreach (XmlNode ns in nss) {
                Canvas nsCanvas = GetNSCanvas();
                nsCanvas.PreviewMouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(ns, nsCanvas);
                this.canvasToNode.Add(nsCanvas, ns);
                settingspanel.Children.Add(nsCanvas);
            }

            int i = 0;
            XmlNodeList pipes = xmlDoc.SelectNodes("//pipe");
            foreach (XmlNode pipeNode in pipes) {

                Canvas canTop = new Canvas();
                canTop.Height = 200;
                canTop.Width = this.panel.Width;
                canTop.SetValue(Canvas.LeftProperty, (double)0);

                Canvas pipeCan = new Canvas() {
                    Height = 24,
                    Width = (double)canTop.Width / 3 + 4,
                    Background = transBrush,
                };
                pipeCan.SetValue(Canvas.LeftProperty, (double)canTop.Width / 3 - 2);
                pipeCan.SetValue(Canvas.TopProperty, canTop.Height / 2 - 12);
                pipeCan.PreviewMouseDown += Can_MouseDown;

                this.nodeToCanvas.Add(pipeNode, pipeCan);
                this.canvasToNode.Add(pipeCan, pipeNode);

                Rectangle rect = new Rectangle() {
                    Width = pipeCan.Width - 10,
                    Height = 16,
                };



                rect.SetValue(Canvas.LeftProperty, 5.0);
                rect.SetValue(Canvas.TopProperty, 4.0);
                rect.Fill = bluishBrush;
                rect.StrokeThickness = 1;
                rect.Stroke = blackBrush;

                TextBlock tb = new TextBlock();
                tb.Text = pipeNode.Attributes["name"].Value;
                tb.FontSize = 12;
                tb.SetValue(Canvas.LeftProperty, 15.0);
                tb.SetValue(Canvas.TopProperty, 4.0);




                Ellipse end = new Ellipse() {
                    Height = 16,
                    Width = 10,
                };
                end.SetValue(Canvas.LeftProperty, 0.0);
                end.SetValue(Canvas.TopProperty, 4.0);
                end.Fill = whiteBrush;
                end.StrokeThickness = 1;
                end.Stroke = blackBrush;

                Ellipse end2 = new Ellipse() {
                    Height = 16,
                    Width = 10,
                };
                end2.SetValue(Canvas.LeftProperty, (double)rect.Width);
                end2.SetValue(Canvas.TopProperty, 4.0);
                end2.Fill = whiteBrush;

                end2.StrokeThickness = 1;
                end2.Stroke = blackBrush;


                pipeCan.Children.Add(rect);
                pipeCan.Children.Add(end);
                pipeCan.Children.Add(end2);
                pipeCan.Children.Add(tb);
               

                canTop.Children.Add(pipeCan);


                XmlNodeList inputNodes = pipeNode.SelectNodes("input");
                int numInputs = inputNodes.Count;

                if (numInputs > 0) {

                    Tuple<int, int> si = GetSizing(inputNodes.Count, 200.0);
                    double imHeight = (double)si.Item1;
                    double space = (double)si.Item2;

                    int inNum = 0;
                    foreach (XmlNode inNode in inputNodes) {
                        Canvas can = GetInputCanvas((double)imHeight, inNode.Attributes["type"].Value);
                        can.SetValue(Canvas.LeftProperty, (double)10);
                        can.SetValue(Canvas.TopProperty, (space / 2 + (space + imHeight) * inNum));

                        canvasToNode.Add(can, inNode);
                        nodeToCanvas.Add(inNode, can);
                        can.PreviewMouseDown += Can_MouseDown;
                        can.Background = transBrush;

                        canTop.Children.Add(can);

                        Path arrowpath = GetResourceCopy<Path>("arrow");
                        double startArrrowY = (double)can.GetValue(Canvas.TopProperty) + imHeight / 2;
                        double startArrrowX = (double)can.GetValue(Canvas.LeftProperty) + can.Width + 4;

                        Tuple<Polygon, Path> p = drawLineArrow(new Point(startArrrowX, startArrrowY), new Point(this.panel.Width * 0.33, 100), arrowpath);

                        canTop.Children.Add(p.Item1);
                        canTop.Children.Add(p.Item2);

                        inNum++;
                    }
                }

                XmlNodeList outputNodes = pipeNode.SelectNodes("output");
                int numOutputs = outputNodes.Count;

                if (numOutputs > 0) {

                    Tuple<int, int> si = GetSizing(outputNodes.Count, 200.0);
                    double imHeight = (double)si.Item1;
                    double space = (double)si.Item2;

                    int outNum = 0;
                    foreach (XmlNode outNode in outputNodes) {
                        Canvas can = GetOutputCanvas((double)imHeight, outNode.Attributes["type"].Value);

                        can.SetValue(Canvas.LeftProperty, (double)(this.panel.Width - imHeight - 10));
                        can.SetValue(Canvas.TopProperty, (space / 2 + (space + imHeight) * outNum));

                        canvasToNode.Add(can, outNode);
                        nodeToCanvas.Add(outNode, can);

                        //can.MouseDown += Can_MouseDown;
                        can.PreviewMouseDown += Can_MouseDown;
                        can.Background = transBrush;


                        canTop.Children.Add(can);

                        Path arrowpath = GetResourceCopy<Path>("arrow");

                        double stopArrrowY = (double)can.GetValue(Canvas.TopProperty) + imHeight / 2;
                        double stopArrrowX = (double)can.GetValue(Canvas.LeftProperty) - 4;

                        Tuple<Polygon, Path> p = drawLineArrow(new Point(this.panel.Width * 0.66, 100), new Point(stopArrrowX, stopArrrowY), arrowpath);

                        canTop.Children.Add(p.Item1);
                        canTop.Children.Add(p.Item2);

                        outNum++;
                    }
                }

                this.panel.Children.Add(canTop);
                Separator sep = new Separator() {
                    Opacity = 127,
                    Height = 15
                };
                this.panel.Children.Add(sep);

                i++;
            }
        }

        private void Can_MouseDown(object sender, MouseButtonEventArgs e) {
            if (sender == null) {
                return;
            }
            XmlNode node = this.canvasToNode[sender as Canvas];
            ViewModel.ViewAttributesCommand.Execute(node);
            HightLightCanvas(sender as Canvas);

        }

        public Tuple<int, int> GetSizing(int num, double height) {

            double imHeight = 70;
            double spacing;

            int maxImageHeight = 50;
            int minSpacing = 2;

            if (num * (maxImageHeight + minSpacing) <= height) {
                spacing = (height - num * maxImageHeight) / num;
                return Tuple.Create(maxImageHeight, Convert.ToInt32(spacing));
            }

            imHeight = maxImageHeight;
            while ((num * (imHeight + minSpacing)) > height) {
                imHeight -= 1;
            }

            return Tuple.Create(Convert.ToInt32(imHeight), minSpacing);
        }

        private T GetResourceCopy<T>(string key) {
            T model = (T)FindResource(key);
            return ElementClone<T>(model);
        }
        /// <summary>
        /// Clones an element.
        /// </summary>
        public static T ElementClone<T>(T element) {
            T clone = default(T);
            MemoryStream memStream = ElementToStream(element);
            clone = ElementFromStream<T>(memStream);
            return clone;
        }

        /// <summary>
        /// Saves an element as MemoryStream.
        /// </summary>
        public static MemoryStream ElementToStream(object element) {
            MemoryStream memStream = new MemoryStream();
            XamlWriter.Save(element, memStream);
            return memStream;
        }

        /// <summary>
        /// Rebuilds an element from a MemoryStream.
        /// </summary>
        public static T ElementFromStream<T>(MemoryStream elementAsStream) {
            object reconstructedElement = null;

            if (elementAsStream.CanRead) {
                elementAsStream.Seek(0, SeekOrigin.Begin);
                reconstructedElement = XamlReader.Load(elementAsStream);
                elementAsStream.Close();
            }

            return (T)reconstructedElement;
        }

        private Tuple<Polygon, Path> drawLineArrow(Point startPoint, Point endPoint, Path arrowpath) {

            GeometryGroup geometryGroup = new GeometryGroup();
            //line
            LineGeometry line = new LineGeometry();
            line.StartPoint = startPoint;
            double length = Math.Sqrt(Math.Abs(startPoint.X - endPoint.X) * Math.Abs(startPoint.X - endPoint.X) +
                Math.Abs(startPoint.Y - endPoint.Y) * Math.Abs(startPoint.Y - endPoint.Y));
            Point EndPoint = new Point(startPoint.X + length, startPoint.Y);
            line.EndPoint = new Point(EndPoint.X - arrowHeadLength, EndPoint.Y);


            geometryGroup.Children.Add(line);
            arrowpath.Data = geometryGroup;
            arrowpath.StrokeThickness = 2;
            arrowpath.Fill = Brushes.SteelBlue;

            //rotate
            RotateTransform form = new RotateTransform();
            form.CenterX = startPoint.X;
            form.CenterY = startPoint.Y;
            //calculate the angle 
            double angle = Math.Asin(Math.Abs(startPoint.Y - endPoint.Y) / length);
            double angle2 = 180 / Math.PI * angle;
            //orientation
            if (endPoint.Y > startPoint.Y) {
                angle2 = (endPoint.X > startPoint.X) ? angle2 : (180 - angle2);
            } else {
                angle2 = (endPoint.X > startPoint.X) ? -angle2 : -(180 - angle2);
            }
            form.Angle = angle2;
            arrowpath.RenderTransform = form;

            Point p1P = new Point(EndPoint.X, EndPoint.Y);
            Point p2P = new Point(EndPoint.X - arrowHeadLength, EndPoint.Y - arrowHeadWidth);
            Point p3P = new Point(EndPoint.X - arrowHeadLength, EndPoint.Y + arrowHeadWidth);

            Point p1 = form.Transform(p1P);
            Point p2 = form.Transform(p2P);
            Point p3 = form.Transform(p3P);

            Polygon p = new Polygon();
            p.Stroke = Brushes.Black;
            p.Fill = Brushes.SteelBlue;
            p.StrokeThickness = 2;
            p.HorizontalAlignment = HorizontalAlignment.Left;
            p.VerticalAlignment = VerticalAlignment.Center;
            p.Points = new PointCollection() { p1, p2, p3 };
            //         this.canvas.Children.Add(p);

            return new Tuple<Polygon, Path>(p, arrowpath);


        }


        internal void ControlPropertyChange() {
            viewModel.OnPropertyChanged("XMLText");
            DrawConfig(viewModel.DataModel);
        }

        void TreeEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ViewModel = e.NewValue as TreeEditorViewModel;
            DrawConfig(viewModel.DataModel);
        }

        public TreeEditorViewModel ViewModel {
            get { return viewModel; }
            set {
                viewModel = value;
                this.Dispatcher.BeginInvoke((Action)delegate {
                    this.Cursor = Cursors.Wait;
                    BindUIElementToViewModel();
                    this.Cursor = Cursors.Arrow;
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void xmlTreeView_Selected(object sender, RoutedEventArgs e) {
            XmlNode selectedItem = xmlTreeView.SelectedItem as XmlNode;
            ViewModel.ViewAttributesCommand.Execute(selectedItem);

            //if (selectedItem.Name == "input" || selectedItem.Name == "output" || selectedItem.Name == "logger" || selectedItem.Name == "monitor" || selectedItem.Name == "altqueue") {
            //    nodeEditorCntrl.Content = new NodeControl(selectedItem, this);
            //} else if (selectedItem.Name == "pipe") {
            //    nodeEditorCntrl.Content = new BooleanControl(selectedItem, this);
            //} else if (selectedItem.Name == "namespace") {
            //    nodeEditorCntrl.Content = new NamespaceControl(selectedItem, this);
            //} else {
            //    nodeEditorCntrl.Content = new NodeEndPointEditorView(selectedItem);
            //}
        }

        private void BindUIElementToViewModel() {
            this.DataContext = viewModel;
            viewModel.View = this as IView;
            if (viewModel == null) {
                return;
            }

            XmlDataProvider dataProvider = this.FindResource("xmlDataProvider") as XmlDataProvider;
            dataProvider.Document = viewModel.DataModel;
            dataProvider.Refresh();
            this.xmlTreeView.ContextMenu.Items.Clear();

            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].Command = ViewModel.AddMonitorCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].Command = ViewModel.AddLoggerCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddLogger]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].Command = ViewModel.AddNamespaceCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace]);


            // Add Pipes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].Command = ViewModel.AddPipeCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddPipe]);

            // Input and Output Nodes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].Command = ViewModel.AddInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddInput]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].Command = ViewModel.AddOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddOutput]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].Command = ViewModel.AddFilterCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddFilter]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].Command = ViewModel.AddExpressionCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddExpression]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].Command = ViewModel.AddAltQueueCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].Command = ViewModel.AddDataFilterCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.Delete]);

            ViewModel.AddXmlNode = AddNewNodeFromUI;
            ViewModel.HighlightNodeInUI = HighlightNode;
        }
        public void HighlightNode(XmlNode xmlNode) {
            bool isSelected = false;

            TreeViewItem rootNode = null;
            try {
                rootNode = xmlTreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;

            } catch {

            }
            if (xmlNode == null) {
                isSelected = SelectTreeViewItem(ref rootNode, "");
            } else {
                isSelected = SelectTreeViewItem(ref rootNode, xmlNode);
            }
            if (!isSelected) {
                MessageBox.Show("Could not locate the node.");
            }

            //temp
            //XmlNode childNode = (xmlTreeView.SelectedItem as XmlNode).FirstChild.CloneNode(true);
            //SelectedNode.InsertAfter(childNode, SelectedNode.FirstChild);

        }
        XmlNode AddNewNodeFromUI(XmlNodeType xmlNodeType) {
            AddChildView popup = new AddChildView(this.ViewModel.DataModel, xmlNodeType);
            popup.ShowDialog();
            return popup.NewNode;
        }


        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e) {
            TreeViewItem selectedItem = sender as TreeViewItem;
            if (selectedItem != null) {
                selectedItem.IsSelected = true;
            }
        }


        private bool SelectTreeViewItem(ref TreeViewItem rootNode, XmlNode toBeSelectedNode) {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded) {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            XmlNode tempNode = rootNode.Header as XmlNode;
            if (tempNode == null) {
                return isSelected;
            }
            if (tempNode == toBeSelectedNode)
            //if (string.Compare(tempNode.Name, toBeSelectedNode.Name, true) == 0 && tempNode.NodeType == toBeSelectedNode.NodeType)
            {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            } else {
                for (int i = 0; i < rootNode.Items.Count; i++) {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, toBeSelectedNode);
                    if (isSelected) {
                        break;
                    }
                }
                return isSelected;
            }


        }

        private bool SelectTreeViewItem(ref TreeViewItem rootNode, string elementName) {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded) {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            XmlNode tempNode = rootNode.Header as XmlNode;
            if (tempNode == null) {
                return isSelected;
            }
            if (string.Compare(tempNode.Name, elementName, true) == 0) {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            } else {
                for (int i = 0; i < rootNode.Items.Count; i++) {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, elementName);
                    if (isSelected) {
                        break;
                    }
                }
                return isSelected;
            }
        }

        public void DrawQXConfig() {
            DrawConfig(viewModel.DataModel);
        }

        public void HightLightCanvas(XmlNode node) {
            if (node == null) {
                return;

            }

            Canvas c;
            try {
                c = this.nodeToCanvas[node];

            } catch (Exception) {
                return;
            }

            if (c == null || c == SelectedCanvas) {
                return;
            }
            SolidColorBrush bluishBrush = new SolidColorBrush();
            bluishBrush.Color = Color.FromArgb(255, 255, 127, 200);
            c.Background = bluishBrush;
            SelectedCanvas = c;
            HighlightNode(node);
        }

        public void HightLightCanvas(Canvas can) {
            if (can == null || can == SelectedCanvas) {
                return;
            }
            SolidColorBrush bluishBrush = new SolidColorBrush();
            bluishBrush.Color = Color.FromArgb(255, 255, 127, 200);
            can.Background = bluishBrush;
            SelectedCanvas = can;
            HighlightNode(this.canvasToNode[can]);
        }

        public void UpdateParamBindings(string param) {
            viewModel.OnPropertyChanged(param);
        }

        public void MSMQSource(XmlNode node) {
            viewModel.myGrid = new MSMQ(node, this);
            viewModel.OnPropertyChanged("myGrid");
        }

        public void MQSource(XmlNode node) {
            viewModel.myGrid = new MQ(node, this);
            viewModel.OnPropertyChanged("myGrid");

        }

        /*
         * Updates the title in the graphical representation of the node when the tpye is changed
         */
        public void UpdateSelectedNodecanvas(XmlNode node) {

            // Dont update logger, monitor or namespace
            string name = node.Name;
            if (name == "logger" || name=="monitor" || name == "namespace") {
                return;
            }

            Canvas can = this.nodeToCanvas[node];
            try {

                foreach (var child in can.Children) {
                    if (child is TextBlock) {
                        TextBlock tb = child as TextBlock;
                        tb.Text = node.Attributes["type"].Value;
                    }
                }
            } catch { }
        }

        /*
 * Updates the title in the graphical representation of the pipe when the tpye is changed
 */
        public void UpdateSelectedPipeCanvas(XmlNode node) {
            Canvas can = this.nodeToCanvas[node];
            try {

                foreach (var child in can.Children) {
                    if (child is TextBlock) {
                        TextBlock tb = child as TextBlock;
                        tb.Text = node.Attributes["name"].Value;
                    }
                }
            } catch { }
        }
    }
}
