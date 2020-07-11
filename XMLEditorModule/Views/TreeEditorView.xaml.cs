using QXEditorModule.Common;
using QXEditorModule.GridDefinitions;
using QXEditorModule.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Path = System.Windows.Shapes.Path;

namespace QXEditorModule.Views
{
    public partial class TreeEditorView : UserControl, INotifyPropertyChanged, IView
    {

        private const int arrowHeadWidth = 5;
        private const int arrowHeadLength = 12;

        private MenuItem inputMenuItem;
        private MenuItem outputMenuItem;
        private readonly ContextMenuProvider contextMenuProvider;
        private ContextMenu pipeContextMenu;

        public event PropertyChangedEventHandler PropertyChanged;
        public TreeEditorViewModel viewModel;
        public Dictionary<XmlNode, Canvas> nodeToCanvas = new Dictionary<XmlNode, Canvas>();
        public Dictionary<Canvas, XmlNode> canvasToNode = new Dictionary<Canvas, XmlNode>();
        public Canvas selectedCanvas;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public TreeEditorView()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(TreeEditorView_DataContextChanged);
            contextMenuProvider = new ContextMenuProvider();
            xmlTreeView.ContextMenu = new ContextMenu();
        }

        private void BindUIElementToViewModel()
        {
            //         this.DataContext = viewModel;
            try
            {
                viewModel.View = this as IView;
                if (viewModel == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }


            XmlDataProvider dataProvider = this.FindResource("xmlDataProvider") as XmlDataProvider;
            dataProvider.Document = viewModel.DataModel;
            dataProvider.Refresh();
            this.xmlTreeView.ContextMenu.Items.Clear();
            try
            {
                this.inputMenuItem.Items.Clear();
                this.outputMenuItem.Items.Clear();

            }
            catch (Exception)
            {

            }
            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].Command = ViewModel.AddOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].CommandParameter = XmlNodeType.Element;

            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].Command = ViewModel.AddInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].CommandParameter = XmlNodeType.Element;

            contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQInput].CommandParameter = "MSMQ";

            contextMenuProvider.ContextMenus[ContextMenuType.AddMQInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMQInput].CommandParameter = "MQ";

            contextMenuProvider.ContextMenus[ContextMenuType.AddFileInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddFileInput].CommandParameter = "FILE";

            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPInput].CommandParameter = "HTTP";

            contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaInput].CommandParameter = "KAFKA";

            contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitInput].CommandParameter = "RABBITDEFEX";

            contextMenuProvider.ContextMenus[ContextMenuType.AddTestInput].Command = ViewModel.AddTypeInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddTestInput].CommandParameter = "TESTSOURCE";


            contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQOutput].CommandParameter = "MSMQ";

            contextMenuProvider.ContextMenus[ContextMenuType.AddMQOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMQOutput].CommandParameter = "MQ";

            contextMenuProvider.ContextMenus[ContextMenuType.AddFileOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddFileOutput].CommandParameter = "FILE";

            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPOutput].CommandParameter = "HTTP";

            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPRest].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPRest].CommandParameter = "REST";

            contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaOutput].CommandParameter = "KAFKA";

            contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitOutput].CommandParameter = "RABBITDEFEX";

            contextMenuProvider.ContextMenus[ContextMenuType.AddTCPOutput].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddTCPOutput].CommandParameter = "TCPCLIENT";

            contextMenuProvider.ContextMenus[ContextMenuType.AddSINK].Command = ViewModel.AddTypeOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddSINK].CommandParameter = "SINK";

            contextMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;

            inputMenuItem = contextMenuProvider.ContextMenus[ContextMenuType.AddInput];
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQInput]);
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMQInput]);
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddFileInput]);
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaInput]);
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPInput]);
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitInput]);
            inputMenuItem.Items.Add(new Separator());
            inputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddTestInput]);

            outputMenuItem = contextMenuProvider.ContextMenus[ContextMenuType.AddOutput];
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMSMQOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMQOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddFileOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddKafkaOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddHTTPRest]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddTCPOutput]);
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddRabbitOutput]);
            outputMenuItem.Items.Add(new Separator());
            outputMenuItem.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddSINK]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].Command = ViewModel.AddMonitorCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor]);

            //contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].Command = ViewModel.AddLoggerCommand;
            //contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].CommandParameter = XmlNodeType.Element;
            //this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddLogger]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].Command = ViewModel.AddNamespaceCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings].Command = ViewModel.AddServiceSettingsCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings]);

            // Add Pipes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].Command = ViewModel.AddPipeCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddPipe]);

            // Input and Output Nodes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());
            {
                this.xmlTreeView.ContextMenu.Items.Add(inputMenuItem);
                this.xmlTreeView.ContextMenu.Items.Add(outputMenuItem);
            }

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].Command = ViewModel.AddFilterCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddFilter]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].Command = ViewModel.AddAltQueueCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddAltQueue]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].Command = ViewModel.AddExpressionCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddExpression]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].Command = ViewModel.AddDataFilterCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddDataFilter]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.Delete]);

            ViewModel.HighlightNodeInUI = HighlightNode;
        }

        public Canvas SelectedCanvas {
            get { return this.selectedCanvas; }
            set {

                // If there is a currently selected Canvas, then reset the background
                if (this.selectedCanvas != null)
                {
                    SolidColorBrush brush = new SolidColorBrush
                    {
                        Color = Colors.Transparent
                    };
                    this.selectedCanvas.Background = brush;


                    // Check if the canvas contains the filter icon and reset that if required
                    if (LogicalTreeHelper.FindLogicalNode(this.selectedCanvas, "filter") is Path path)
                    {
                        SolidColorBrush greenBrush = new SolidColorBrush
                        {
                            Color = Color.FromArgb(255, 0, 0, 0)
                        };
                        path.Fill = greenBrush;
                    }
                }

                this.selectedCanvas = value;
            }
        }

        private Canvas GetInputCanvas(double imHeight, string title)
        {

            double fs = 9;
            double h = imHeight - fs - 2;
            double w = h * 0.8563;
            double tY = h;
            double tX = 2;

            Canvas imcanvas = GetResourceCopy<Canvas>("input");
            imcanvas.Height = h;
            imcanvas.Width = w;

            Path path = LogicalTreeHelper.FindLogicalNode(imcanvas, "path") as Path;
            path.Height = h;
            path.Width = w;

            TextBlock txt = new TextBlock
            {
                Text = title,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = fs
            };
            txt.SetValue(Canvas.LeftProperty, tX);
            txt.SetValue(Canvas.TopProperty, tY);

            Canvas can = new Canvas
            {
                Height = imHeight,
                Width = w
            };
            can.Children.Add(imcanvas);
            can.Children.Add(txt);

            return can;
        }

        private Canvas GetOutputCanvas(double imHeight, string title)
        {

            double fs = 9;
            double h = imHeight - fs - 2;
            double w = h;
            double tY = h;
            double tX = 2;

            Canvas imcanvas = GetResourceCopy<Canvas>("input");
            imcanvas.Height = h;
            imcanvas.Width = w;

            Path path = LogicalTreeHelper.FindLogicalNode(imcanvas, "path") as Path;
            path.Height = h;
            path.Width = w;

            //SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            //mySolidColorBrush.Color = Color.FromArgb(255, 128, 128, 128);
            //path.Fill = mySolidColorBrush;

            TextBlock txt = new TextBlock
            {
                Text = title,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = fs
            };
            txt.SetValue(Canvas.LeftProperty, tX);
            txt.SetValue(Canvas.TopProperty, tY);

            Canvas can = new Canvas
            {
                Height = imHeight,
                Width = w
            };
            can.Children.Add(imcanvas);
            can.Children.Add(txt);

            return can;
        }

        public Canvas GetMonitorCanvas()
        {

            Canvas can = new Canvas()
            {
                Height = 44,
                Width = 30
            };
            Canvas monitorcanvas = GetResourceCopy<Canvas>("monitor");

            can.Children.Add(monitorcanvas);

            TextBlock txt = new TextBlock() { Text = "Monitor", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)monitorcanvas.Height + 2);

            can.Children.Add(txt);

            return can;
        }

        public Canvas GetLoggerCanvas()
        {

            Canvas can = new Canvas()
            {
                Height = 44,
                Width = 30
            };
            Canvas monitorcanvas = GetResourceCopy<Canvas>("logger");

            can.Children.Add(monitorcanvas);

            TextBlock txt = new TextBlock() { Text = "Logger", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)monitorcanvas.Height + 2);

            can.Children.Add(txt);

            return can;
        }

        public Canvas GetNSCanvas()
        {
            Canvas can = new Canvas()
            {
                Height = 44,
                Width = 44
            };
            Canvas nscanvas = GetResourceCopy<Canvas>("namespace");

            can.Children.Add(nscanvas);

            TextBlock txt = new TextBlock() { Text = "Namespace", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)32);

            can.Children.Add(txt);

            return can;
        }

        public Canvas GetServiceCanvas()
        {
            Canvas can = new Canvas()
            {
                Height = 44,
                Width = 35
            };
            Canvas nscanvas = GetResourceCopy<Canvas>("service");

            can.Children.Add(nscanvas);

            TextBlock txt = new TextBlock() { Text = "Service", FontSize = 8 };
            txt.SetValue(Canvas.LeftProperty, 0.0);
            txt.SetValue(Canvas.TopProperty, (double)32);

            can.Children.Add(txt);

            return can;
        }
        public void DrawConfig(XmlDocument xmlDoc)
        {

            this.panel.Children.Clear();
            this.settingspanel.Children.Clear();
            this.canvasToNode.Clear();
            this.nodeToCanvas.Clear();

            SolidColorBrush blackBrush = new SolidColorBrush
            {
                Color = Colors.Black
            };

            SolidColorBrush whiteBrush = new SolidColorBrush
            {
                Color = Colors.White
            };

            SolidColorBrush bluishBrush = new SolidColorBrush
            {
                Color = Color.FromArgb(255, 135, 206, 250)
            };

            SolidColorBrush transBrush = new SolidColorBrush
            {
                Color = Colors.Transparent
            };

            SolidColorBrush aliceBrush = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };


            XmlNodeList monitors = xmlDoc.SelectNodes("//monitor");
            foreach (XmlNode monitor in monitors)
            {
                Canvas monitorCanvas = GetMonitorCanvas();
                monitorCanvas.MouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(monitor, monitorCanvas);
                this.canvasToNode.Add(monitorCanvas, monitor);
                settingspanel.Children.Add(monitorCanvas);

                monitorCanvas.ContextMenu = new ContextMenu();
                ContextMenuProvider monMenuProvider = new ContextMenuProvider();
                monMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
                monitorCanvas.ContextMenu.Items.Add(monMenuProvider.ContextMenus[ContextMenuType.Delete]);
            }

            XmlNodeList srvs = xmlDoc.SelectNodes("//service");
            foreach (XmlNode ns in srvs)
            {
                Canvas srvCanvas = GetServiceCanvas();
                srvCanvas.PreviewMouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(ns, srvCanvas);
                this.canvasToNode.Add(srvCanvas, ns);
                settingspanel.Children.Add(srvCanvas);

                srvCanvas.ContextMenu = new ContextMenu();
                ContextMenuProvider srvMenuProvider = new ContextMenuProvider();
                srvMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
                srvCanvas.ContextMenu.Items.Add(srvMenuProvider.ContextMenus[ContextMenuType.Delete]);
            }

            XmlNodeList nss = xmlDoc.SelectNodes("//namespace");
            foreach (XmlNode ns in nss)
            {
                Canvas nsCanvas = GetNSCanvas();
                nsCanvas.PreviewMouseDown += Can_MouseDown;
                this.nodeToCanvas.Add(ns, nsCanvas);
                this.canvasToNode.Add(nsCanvas, ns);
                settingspanel.Children.Add(nsCanvas);

                nsCanvas.ContextMenu = new ContextMenu();
                ContextMenuProvider nsMenuProvider = new ContextMenuProvider();
                nsMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
                nsCanvas.ContextMenu.Items.Add(nsMenuProvider.ContextMenus[ContextMenuType.Delete]);
            }

            // Context Menu for the Settings Panel 
            this.settingspanel.ContextMenu = new ContextMenu();
            ContextMenuProvider setMenuProvider = new ContextMenuProvider();

            setMenuProvider.ContextMenus[ContextMenuType.AddMonitor].Command = ViewModel.AddMonitorCommand;
            setMenuProvider.ContextMenus[ContextMenuType.AddMonitor].CommandParameter = XmlNodeType.Element;
            this.settingspanel.ContextMenu.Items.Add(setMenuProvider.ContextMenus[ContextMenuType.AddMonitor]);

            setMenuProvider.ContextMenus[ContextMenuType.AddNamespace].Command = ViewModel.AddNamespaceCommand;
            setMenuProvider.ContextMenus[ContextMenuType.AddNamespace].CommandParameter = XmlNodeType.Element;
            this.settingspanel.ContextMenu.Items.Add(setMenuProvider.ContextMenus[ContextMenuType.AddNamespace]);

            setMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings].Command = ViewModel.AddServiceSettingsCommand;
            setMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings].CommandParameter = XmlNodeType.Element;
            this.settingspanel.ContextMenu.Items.Add(setMenuProvider.ContextMenus[ContextMenuType.AddServiceSettings]);

            this.settingspanel.PreviewMouseRightButtonDown += delegate (object sender, MouseButtonEventArgs e) { CanCanvas_MouseDown(sender, xmlDoc.SelectSingleNode("//settings")); };

            int i = 0;
            XmlNodeList pipes = xmlDoc.SelectNodes("//pipe");
            foreach (XmlNode pipeNode in pipes)
            {

                Canvas canTop = new Canvas
                {
                    Height = 200,
                    Width = this.panel.Width,
                    Background = transBrush
                };
                canTop.SetValue(Canvas.LeftProperty, (double)0);

                canTop.ContextMenu = new ContextMenu();
                ContextMenuProvider topMenuProvider = new ContextMenuProvider();
                topMenuProvider.ContextMenus[ContextMenuType.AddPipe].Command = ViewModel.AddPipeCommand;
                topMenuProvider.ContextMenus[ContextMenuType.AddPipe].CommandParameter = XmlNodeType.Element;
                canTop.ContextMenu.Items.Add(topMenuProvider.ContextMenus[ContextMenuType.AddPipe]);
                canTop.PreviewMouseRightButtonDown += delegate (object sender, MouseButtonEventArgs e) { CanCanvas_MouseDown(sender, xmlDoc.SelectSingleNode("//pipes")); };

                Canvas pipeCan = new Canvas()
                {
                    Height = 24,
                    Width = (double)canTop.Width / 3 + 4,
                    Background = transBrush,
                };
                pipeCan.SetValue(Canvas.LeftProperty, (double)canTop.Width / 3 - 2);
                pipeCan.SetValue(Canvas.TopProperty, canTop.Height / 2 - 12);

                pipeCan.PreviewMouseDown += Can_MouseDown;

                this.nodeToCanvas.Add(pipeNode, pipeCan);
                this.canvasToNode.Add(pipeCan, pipeNode);

                Rectangle rect = new Rectangle()
                {
                    Width = pipeCan.Width - 10,
                    Height = 16,
                };

                rect.SetValue(Canvas.LeftProperty, 5.0);
                rect.SetValue(Canvas.TopProperty, 4.0);
                rect.Fill = bluishBrush;
                rect.StrokeThickness = 1;
                rect.Stroke = blackBrush;

                TextBlock tb = new TextBlock
                {
                    Text = pipeNode.Attributes["name"].Value,
                    FontSize = 12
                };
                tb.SetValue(Canvas.LeftProperty, 15.0);
                tb.SetValue(Canvas.TopProperty, 4.0);

                Ellipse end = new Ellipse()
                {
                    Height = 16,
                    Width = 10,
                };
                end.SetValue(Canvas.LeftProperty, 0.0);
                end.SetValue(Canvas.TopProperty, 4.0);
                end.Fill = whiteBrush;
                end.StrokeThickness = 1;
                end.Stroke = blackBrush;

                Ellipse end2 = new Ellipse()
                {
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

                pipeCan.ContextMenu = pipeContextMenu;


                canTop.Children.Add(pipeCan);

                // Draw the input Nodes
                XmlNodeList inputNodes = pipeNode.SelectNodes("input");
                int numInputs = inputNodes.Count;

                if (numInputs > 0)
                {

                    Tuple<int, int> si = GetSizing(inputNodes.Count, 200.0);
                    double imHeight = (double)si.Item1;
                    double space = (double)si.Item2;

                    int inNum = 0;
                    foreach (XmlNode inNode in inputNodes)
                    {
                        ConstructNode(canTop, inNode, imHeight, space, inNum, true);
                        inNum++;
                    }
                }


                // Draw the output nodes
                XmlNodeList outputNodes = pipeNode.SelectNodes("output");
                int numOutputs = outputNodes.Count;

                if (numOutputs > 0)
                {
                    // The size of the nodes
                    Tuple<int, int> si = GetSizing(outputNodes.Count, 200.0);
                    double imHeight = (double)si.Item1;
                    double space = (double)si.Item2;

                    int outNum = 0;
                    foreach (XmlNode outNode in outputNodes)
                    {
                        ConstructNode(canTop, outNode, imHeight, space, outNum, false);
                        outNum++;
                    }
                }

                this.panel.Children.Add(canTop);
                i++;
            }
        }

        private ContextMenu GetPipeContextMenu()
        {

            ContextMenu ctx = new ContextMenu();
            ContextMenuProvider menuProvider = new ContextMenuProvider();

            //            menuProvider.ContextMenus[ContextMenuType.AddInput].Command = ViewModel.AddInputCommand;
            //          menuProvider.ContextMenus[ContextMenuType.AddInput].CommandParameter = XmlNodeType.Element;

            menuProvider.ContextMenus[ContextMenuType.AddOutput].Command = ViewModel.AddOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddOutput].CommandParameter = XmlNodeType.Element;

            menuProvider.ContextMenus[ContextMenuType.AddMSMQInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddMSMQInput].CommandParameter = "MSMQ";

            menuProvider.ContextMenus[ContextMenuType.AddMQInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddMQInput].CommandParameter = "MQ";

            menuProvider.ContextMenus[ContextMenuType.AddFileInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddFileInput].CommandParameter = "FILE";

            menuProvider.ContextMenus[ContextMenuType.AddHTTPInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddHTTPInput].CommandParameter = "HTTP";

            menuProvider.ContextMenus[ContextMenuType.AddKafkaInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddKafkaInput].CommandParameter = "KAFKA";

            menuProvider.ContextMenus[ContextMenuType.AddRabbitInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddRabbitInput].CommandParameter = "RABBITDEFEX";

            menuProvider.ContextMenus[ContextMenuType.AddTestInput].Command = ViewModel.AddTypeInputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddTestInput].CommandParameter = "TESTSOURCE";


            menuProvider.ContextMenus[ContextMenuType.AddMSMQOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddMSMQOutput].CommandParameter = "MSMQ";

            menuProvider.ContextMenus[ContextMenuType.AddMQOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddMQOutput].CommandParameter = "MQ";

            menuProvider.ContextMenus[ContextMenuType.AddFileOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddFileOutput].CommandParameter = "FILE";

            menuProvider.ContextMenus[ContextMenuType.AddHTTPOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddHTTPOutput].CommandParameter = "HTTP";

            menuProvider.ContextMenus[ContextMenuType.AddHTTPRest].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddHTTPRest].CommandParameter = "REST";

            menuProvider.ContextMenus[ContextMenuType.AddTCPOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddTCPOutput].CommandParameter = "TCPCLIENT";

            menuProvider.ContextMenus[ContextMenuType.AddKafkaOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddKafkaOutput].CommandParameter = "KAFKA";

            menuProvider.ContextMenus[ContextMenuType.AddRabbitOutput].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddRabbitOutput].CommandParameter = "RABBITDEFEX";

            //           menuProvider.ContextMenus[ContextMenuType.AddAMSMVTUpdatedOutput].Command = ViewModel.AddTypeOutputCommand;
            //           menuProvider.ContextMenus[ContextMenuType.AddAMSMVTUpdatedOutput].CommandParameter = "RABBITDEFEX";

            menuProvider.ContextMenus[ContextMenuType.AddSINK].Command = ViewModel.AddTypeOutputCommand;
            menuProvider.ContextMenus[ContextMenuType.AddSINK].CommandParameter = "SINK";

            menuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;

            MenuItem inputMenuItem = menuProvider.ContextMenus[ContextMenuType.AddInput];
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddMSMQInput]);
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddMQInput]);
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddFileInput]);
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddKafkaInput]);
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddHTTPInput]);
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddRabbitInput]);
            inputMenuItem.Items.Add(new Separator());
            inputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddTestInput]);

            MenuItem outputMenuItem = menuProvider.ContextMenus[ContextMenuType.AddOutput];
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddMSMQOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddMQOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddFileOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddKafkaOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddHTTPOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddHTTPRest]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddTCPOutput]);
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddRabbitOutput]);
            outputMenuItem.Items.Add(new Separator());
            outputMenuItem.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddSINK]);


            //          ctx.Items.Add(menuProvider.ContextMenus[ContextMenuType.AddInput]);
            ctx.Items.Add(inputMenuItem);
            ctx.Items.Add(outputMenuItem);
            ctx.Items.Add(new Separator());

            ctx.Items.Add(menuProvider.ContextMenus[ContextMenuType.Delete]);

            return ctx;

        }

        private void ConstructNode(Canvas canTop, XmlNode node, double imHeight, double space, int index, bool inNode)
        {

            SolidColorBrush blackBrush = new SolidColorBrush
            {
                Color = Colors.Black
            };

            SolidColorBrush transBrush = new SolidColorBrush
            {
                Color = Colors.Transparent
            };

            //The node canvas
            Canvas can = GetOutputCanvas((double)imHeight, node.Attributes["type"].Value);

            if (inNode)
            {
                can = GetInputCanvas((double)imHeight, node.Attributes["type"].Value);
            }

            if (inNode)
            {
                can.SetValue(Canvas.LeftProperty, (double)10);
            }
            else
            {
                can.SetValue(Canvas.LeftProperty, (double)(this.panel.Width - imHeight - 10));
            }
            can.SetValue(Canvas.TopProperty, (space / 2 + (space + imHeight) * index));

            // Add the event handlers
            canvasToNode.Add(can, node);
            nodeToCanvas.Add(node, can);
            can.PreviewMouseDown += Can_MouseDown;
            can.Background = transBrush;

            bool hasFilter = node.HasChildNodes;
            bool hasAltQueue = false;
            if (hasFilter)
            {
                hasAltQueue = node.FirstChild.SelectNodes("./altqueue").Item(0) != null;
            }
            bool hasStyle = node.Attributes["stylesheet"] != null;

            double filterCanvasOffset = 0.0;

            // Add Filter indicator if present
            if (hasFilter)
            {

                // Calculate it's position
                if (inNode)
                {
                    filterCanvasOffset = (double)can.GetValue(Canvas.LeftProperty) + can.Width + 4.0;
                }

                if (!inNode)
                {
                    if (hasStyle)
                    {
                        filterCanvasOffset = (double)can.GetValue(Canvas.LeftProperty) - can.Width / 2 - 24.0;
                    }
                    else
                    {
                        filterCanvasOffset = (double)can.GetValue(Canvas.LeftProperty) - can.Width / 2 - 4.0;
                    }

                    if (hasAltQueue)
                    {
                        filterCanvasOffset -= 20.0;
                    }
                }
                double altCanvasOffset = filterCanvasOffset + 20.0;

                XmlNode filterNode = node.FirstChild;

                Canvas filterCanvas = new Canvas()
                {
                    Height = can.Height / 2,
                    Width = can.ActualWidth / 2,
                    Background = blackBrush
                };

                filterCanvas.SetValue(Canvas.TopProperty, (double)can.GetValue(Canvas.TopProperty));
                filterCanvas.SetValue(Canvas.LeftProperty, filterCanvasOffset);

                filterCanvas.Children.Add(GetResourceCopy<Path>("filter"));
                canTop.Children.Add(filterCanvas);

                // Add the event handlers
                canvasToNode.Add(filterCanvas, node.FirstChild);
                nodeToCanvas.Add(node.FirstChild, filterCanvas);
                filterCanvas.MouseDown += Can_MouseDown;
                filterCanvas.Background = transBrush;

                filterCanvas.ContextMenu = new ContextMenu();
                ContextMenuProvider filMenuProvider = new ContextMenuProvider();

                filMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].Command = ViewModel.AddAltQueueCommand;
                filMenuProvider.ContextMenus[ContextMenuType.AddAltQueue].CommandParameter = XmlNodeType.Element;
                filterCanvas.ContextMenu.Items.Add(filMenuProvider.ContextMenus[ContextMenuType.AddAltQueue]);

                filMenuProvider.ContextMenus[ContextMenuType.AddExpression].Command = ViewModel.AddExpressionCommand;
                filMenuProvider.ContextMenus[ContextMenuType.AddExpression].CommandParameter = XmlNodeType.Element;
                filterCanvas.ContextMenu.Items.Add(filMenuProvider.ContextMenus[ContextMenuType.AddExpression]);

                filMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].Command = ViewModel.AddDataFilterCommand;
                filMenuProvider.ContextMenus[ContextMenuType.AddDataFilter].CommandParameter = XmlNodeType.Element;
                filterCanvas.ContextMenu.Items.Add(filMenuProvider.ContextMenus[ContextMenuType.AddDataFilter]);

                filterCanvas.ContextMenu.Items.Add(new Separator());

                filMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
                filterCanvas.ContextMenu.Items.Add(filMenuProvider.ContextMenus[ContextMenuType.Delete]);


                // Alt Queue node

                XmlNode altqueue = filterNode.SelectNodes("./altqueue").Item(0);

                if (hasAltQueue)
                {

                    Canvas altCanvas = new Canvas()
                    {
                        Height = 25,
                        Width = 25,
                        Background = transBrush
                    };

                    altCanvas.SetValue(Canvas.LeftProperty, altCanvasOffset);
                    altCanvas.SetValue(Canvas.TopProperty, (double)can.GetValue(Canvas.TopProperty) - 15.0);

                    altCanvas.Children.Add(GetResourceCopy<Path>("altqueue"));
                    canTop.Children.Add(altCanvas);
                    altCanvas.MouseDown += Can_MouseDown;

                    // Context Menu to provide "Delete" selection
                    altCanvas.ContextMenu = new ContextMenu();
                    ContextMenuProvider altMenuProvider = new ContextMenuProvider();
                    altMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
                    altCanvas.ContextMenu.Items.Add(altMenuProvider.ContextMenus[ContextMenuType.Delete]);

                    // Enable it to be highlighted/selected
                    canvasToNode.Add(altCanvas, altqueue);
                    nodeToCanvas.Add(altqueue, altCanvas);

                }
            }

            // Add stylesheet indicator if present
            if (hasStyle)
            {

                double styleCanvasOffset = 0.0;

                // Calculate it's position
                if (inNode)
                {
                    if (hasFilter)
                    {
                        styleCanvasOffset = filterCanvasOffset + 24.0;
                        if (hasAltQueue)
                        {
                            styleCanvasOffset += 20.0;
                        }
                    }
                    else
                    {
                        styleCanvasOffset = (double)can.GetValue(Canvas.LeftProperty) + can.Width + 4.0;
                    }
                }

                if (!inNode)
                {
                    styleCanvasOffset = (double)can.GetValue(Canvas.LeftProperty) - 20.0;
                }


                Canvas transformCanvas = new Canvas()
                {
                    Height = can.Height / 2,
                    Width = can.ActualWidth / 2,
                    Background = blackBrush
                };

                transformCanvas.SetValue(Canvas.TopProperty, (double)can.GetValue(Canvas.TopProperty));
                transformCanvas.SetValue(Canvas.LeftProperty, styleCanvasOffset);

                Path transformpath = GetResourceCopy<Path>("transform");
                transformCanvas.Children.Add(transformpath);

                canTop.Children.Add(transformCanvas);
            }

            // Add the context menu handler
            can.ContextMenu = new ContextMenu();
            ContextMenuProvider inMenuProvider = new ContextMenuProvider();
            inMenuProvider.ContextMenus[ContextMenuType.AddFilter].Command = ViewModel.AddFilterCommand;
            inMenuProvider.ContextMenus[ContextMenuType.AddFilter].CommandParameter = XmlNodeType.Element;
            can.ContextMenu.Items.Add(inMenuProvider.ContextMenus[ContextMenuType.AddFilter]);

            can.ContextMenu.Items.Add(new Separator());

            inMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
            can.ContextMenu.Items.Add(inMenuProvider.ContextMenus[ContextMenuType.Delete]);

            // Add it to the parent canvas
            canTop.Children.Add(can);

            // Add the arrow
            Path arrowpath = GetResourceCopy<Path>("arrow");
            Tuple<Polygon, Path> p;


            if (inNode)
            {
                double startArrrowY = (double)can.GetValue(Canvas.TopProperty) + imHeight / 2;
                double startArrrowX = (double)can.GetValue(Canvas.LeftProperty) + can.Width + 4;
                p = DrawLineArrow(new Point(startArrrowX, startArrrowY), new Point(this.panel.Width * 0.33, 100), arrowpath);
            }
            else
            {
                double stopArrrowY = (double)can.GetValue(Canvas.TopProperty) + imHeight / 2;
                double stopArrrowX = (double)can.GetValue(Canvas.LeftProperty) - 4;
                p = DrawLineArrow(new Point(this.panel.Width * 0.66, 100), new Point(stopArrrowX, stopArrrowY), arrowpath);
            }

            canTop.Children.Add(p.Item1);
            canTop.Children.Add(p.Item2);

        }
        private void Can_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (sender == null)
            {
                return;
            }
            XmlNode node = this.canvasToNode[sender as Canvas];
            ViewModel.ViewAttributesCommand.Execute(node);
            HightLightCanvas(sender as Canvas);

            if (LogicalTreeHelper.FindLogicalNode(sender as Canvas, "filter") is Path path)
            {
                SolidColorBrush brush = new SolidColorBrush
                {
                    Color = Colors.Red
                };
                path.Fill = brush;
            }
            if (LogicalTreeHelper.FindLogicalNode(sender as Canvas, "outputnode") is Path path2)
            {
                SolidColorBrush brush = new SolidColorBrush
                {
                    Color = Colors.Red
                };
                path2.Fill = brush;
            }
        }

        private void CanCanvas_MouseDown(object sender, XmlNode node)
        {
            if (sender == null)
            {
                return;
            }
            ViewModel.ViewAttributesCommand.Execute(node);
            HighlightNode(node);
        }

        public Tuple<int, int> GetSizing(int num, double height)
        {

            double spacing;

            int maxImageHeight = 50;
            int minSpacing = 2;

            if (num * (maxImageHeight + minSpacing) <= height)
            {
                spacing = (height - num * maxImageHeight) / num;
                return Tuple.Create(maxImageHeight, Convert.ToInt32(spacing));
            }

            double imHeight = maxImageHeight;
            while ((num * (imHeight + minSpacing)) > height)
            {
                imHeight -= 1;
            }

            return Tuple.Create(Convert.ToInt32(imHeight), minSpacing);
        }

        private T GetResourceCopy<T>(string key)
        {
            T model = (T)FindResource(key);
            return ElementClone<T>(model);
        }
        /// <summary>
        /// Clones an element.
        /// </summary>
        public static T ElementClone<T>(T element)
        {
            //T clone = default(T);
            MemoryStream memStream = ElementToStream(element);
            T clone = ElementFromStream<T>(memStream);
            return clone;
        }

        /// <summary>
        /// Saves an element as MemoryStream.
        /// </summary>
        public static MemoryStream ElementToStream(object element)
        {
            MemoryStream memStream = new MemoryStream();
            XamlWriter.Save(element, memStream);
            return memStream;
        }

        /// <summary>
        /// Rebuilds an element from a MemoryStream.
        /// </summary>
        public static T ElementFromStream<T>(MemoryStream elementAsStream)
        {
            object reconstructedElement = null;

            if (elementAsStream.CanRead)
            {
                elementAsStream.Seek(0, SeekOrigin.Begin);
                reconstructedElement = XamlReader.Load(elementAsStream);
                elementAsStream.Close();
            }

            return (T)reconstructedElement;
        }

        private Tuple<Polygon, Path> DrawLineArrow(Point startPoint, Point endPoint, Path arrowpath)
        {

            GeometryGroup geometryGroup = new GeometryGroup();
            //line
            LineGeometry line = new LineGeometry
            {
                StartPoint = startPoint
            };
            double length = Math.Sqrt(Math.Abs(startPoint.X - endPoint.X) * Math.Abs(startPoint.X - endPoint.X) +
                Math.Abs(startPoint.Y - endPoint.Y) * Math.Abs(startPoint.Y - endPoint.Y));
            Point EndPoint = new Point(startPoint.X + length, startPoint.Y);
            line.EndPoint = new Point(EndPoint.X - arrowHeadLength, EndPoint.Y);


            geometryGroup.Children.Add(line);
            arrowpath.Data = geometryGroup;
            arrowpath.StrokeThickness = 2;
            arrowpath.Fill = Brushes.SteelBlue;

            //rotate
            RotateTransform form = new RotateTransform
            {
                CenterX = startPoint.X,
                CenterY = startPoint.Y
            };
            //calculate the angle 
            double angle = Math.Asin(Math.Abs(startPoint.Y - endPoint.Y) / length);
            double angle2 = 180 / Math.PI * angle;
            //orientation
            if (endPoint.Y > startPoint.Y)
            {
                angle2 = (endPoint.X > startPoint.X) ? angle2 : (180 - angle2);
            }
            else
            {
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

            Polygon p = new Polygon
            {
                Stroke = Brushes.Black,
                Fill = Brushes.SteelBlue,
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Points = new PointCollection() { p1, p2, p3 }
            };
            //         this.canvas.Children.Add(p);

            return new Tuple<Polygon, Path>(p, arrowpath);


        }

        public void RefreshDraw()
        {
            DrawConfig(viewModel.DataModel);
        }
        internal void ControlPropertyChange()
        {
            viewModel.OnPropertyChanged("XMLText");
            DrawConfig(viewModel.DataModel);
        }

        void TreeEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = e.NewValue as TreeEditorViewModel;
            if (ViewModel != null)
            {
                pipeContextMenu = GetPipeContextMenu();
                DrawConfig(viewModel.DataModel);
            }
        }

        public TreeEditorViewModel ViewModel {
            get { return viewModel; }
            set {
                viewModel = value;
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    this.Cursor = Cursors.Wait;
                    BindUIElementToViewModel();
                    this.Cursor = Cursors.Arrow;
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        #region Selection Handling
        private void XmlTreeView_Selected(object sender, RoutedEventArgs e)
        {
            XmlNode selectedItem = xmlTreeView.SelectedItem as XmlNode;
            ViewModel.ViewAttributesCommand.Execute(selectedItem);
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is TreeViewItem selectedItem)
            {
                selectedItem.IsSelected = true;
            }
        }

        private bool SelectTreeViewItem(ref TreeViewItem rootNode, XmlNode toBeSelectedNode)
        {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded)
            {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            if (!(rootNode.Header is XmlNode tempNode))
            {
                return isSelected;
            }
            if (tempNode == toBeSelectedNode)
            //if (string.Compare(tempNode.Name, toBeSelectedNode.Name, true) == 0 && tempNode.NodeType == toBeSelectedNode.NodeType)
            {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            }
            else
            {
                for (int i = 0; i < rootNode.Items.Count; i++)
                {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, toBeSelectedNode);
                    if (isSelected)
                    {
                        break;
                    }
                }
                return isSelected;
            }


        }

        private bool SelectTreeViewItem(ref TreeViewItem rootNode, string elementName)
        {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded)
            {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            if (!(rootNode.Header is XmlNode tempNode))
            {
                return isSelected;
            }
            if (string.Compare(tempNode.Name, elementName, true) == 0)
            {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            }
            else
            {
                for (int i = 0; i < rootNode.Items.Count; i++)
                {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, elementName);
                    if (isSelected)
                    {
                        break;
                    }
                }
                return isSelected;
            }
        }

        #endregion


        public void DrawQXConfig()
        {
            DrawConfig(viewModel.DataModel);
        }

        public void HightLightCanvas(XmlNode node)
        {



            if (node == null)
            {
                return;
            }

            Canvas c;
            try
            {
                c = this.nodeToCanvas[node];
            }
            catch (Exception)
            {

                if (this.selectedCanvas != null)
                {
                    SolidColorBrush brush = new SolidColorBrush
                    {
                        Color = Colors.Transparent
                    };
                    this.selectedCanvas.Background = brush;
                }
                return;
            }

            if (c == null)
            {
                return;
            }

            if (c == SelectedCanvas)
            {
                return;
            }

            SolidColorBrush highlightBrush = new SolidColorBrush();

            switch (node.Name)
            {
                case "output":
                    highlightBrush.Color = Color.FromArgb(127, 255, 0, 0);
                    break;
                case "input":
                    highlightBrush.Color = Color.FromArgb(127, 0, 255, 0);
                    break;
                case "pipe":
                    highlightBrush.Color = Color.FromArgb(127, 0, 0, 255);
                    break;
                case "filter":
                    if (LogicalTreeHelper.FindLogicalNode(c, "filter") is Path path)
                    {
                        SolidColorBrush brush = new SolidColorBrush
                        {
                            Color = Colors.DarkOrange
                        };
                        path.Fill = brush;
                    }
                    break;
                default:
                    highlightBrush.Color = Color.FromArgb(255, 255, 127, 200);
                    break;
            }

            highlightBrush.Color = Colors.DarkOrange;


            c.Background = highlightBrush;

            SelectedCanvas = c;
            HighlightNode(node);
        }

        public void HightLightCanvas(Canvas can)
        {
            if (can == null || can == SelectedCanvas)
            {
                return;
            }
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = Color.FromArgb(255, 255, 127, 200)
            };
            can.Background = brush;
            SelectedCanvas = can;
            HighlightNode(this.canvasToNode[can]);
        }

        public void HighlightNode(XmlNode xmlNode)
        {

            TreeViewItem rootNode = null;
            try
            {
                rootNode = xmlTreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
            }
            catch
            {

            }
            if (xmlNode == null)
            {
                SelectTreeViewItem(ref rootNode, "");
            }
            else
            {
                SelectTreeViewItem(ref rootNode, xmlNode);
            }
        }

        public void UpdateParamBindings(string param)
        {
            viewModel.OnPropertyChanged(param);
        }

        #region updatePropertyGrid

        public void MSMQIn(XmlNode node)
        {
            viewModel.MyGrid = new MSMQIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void MSMQOut(XmlNode node)
        {
            viewModel.MyGrid = new MSMQOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void MQInSource(XmlNode node)
        {
            viewModel.MyGrid = new MQIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void MQOutSource(XmlNode node)
        {
            viewModel.MyGrid = new MQOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void FileInSource(XmlNode node)
        {
            viewModel.MyGrid = new FILEIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }
        public void FileOutSource(XmlNode node)
        {
            viewModel.MyGrid = new FILEOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }
        public void KafkaIn(XmlNode node)
        {
            viewModel.MyGrid = new KAFKAIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }
        public void KafkaOut(XmlNode node)
        {
            viewModel.MyGrid = new KAFKAOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void RestOut(XmlNode node)
        {
            viewModel.MyGrid = new RESTOUT(node, this);
            viewModel.OnPropertyChanged("myGrid");
        }

        public void HTTPOut(XmlNode node)
        {
            viewModel.MyGrid = new HTTPOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }
        public void HTTPIn(XmlNode node)
        {
            viewModel.MyGrid = new HTTPIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }
        public void RabbitOut(XmlNode node)
        {
            viewModel.MyGrid = new RABBITOUT(node, this);
            viewModel.OnPropertyChanged("myGrid");
        }

        public void RabbitIn(XmlNode node)
        {
            viewModel.MyGrid = new RABBITIN(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void SinkOut(XmlNode node)
        {
            viewModel.MyGrid = new SINK(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void TestSource(XmlNode node)
        {
            viewModel.MyGrid = new TESTSOURCE(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        public void TCPOUT(XmlNode node)
        {
            viewModel.MyGrid = new TCPOUT(node, this);
            viewModel.OnPropertyChanged("MyGrid");
        }

        #endregion


        /*
         * Updates the title in the graphical representation of the node when the type is changed
         */
        public void UpdateSelectedNodeCanvas(XmlNode node)
        {

            // Dont update logger, monitor or namespace
            string name = node.Name;
            if (name == "logger" || name == "monitor" || name == "namespace")
            {
                return;
            }


            try
            {
                Canvas can = this.nodeToCanvas[node];
                foreach (var child in can.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock tb = child as TextBlock;
                        tb.Text = node.Attributes["type"].Value;
                    }
                }
            }
            catch { }
        }

        /*
         * Updates the title in the graphical representation of the pipe when the tpye is changed
         */
        public void UpdateSelectedPipeCanvas(XmlNode node)
        {

            try
            {
                Canvas can = this.nodeToCanvas[node];
                foreach (var child in can.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock tb = child as TextBlock;
                        tb.Text = node.Attributes["name"].Value;
                    }
                }
            }
            catch { }
        }

        public void ChangeElementType(string value)
        {
            viewModel.ChangeElementType(value);
        }
        public bool CanChangeElementType(string value)
        {
            return viewModel.CanChangeElementType(value);
        }
        public void ChangeFilterType(string value)
        {
            viewModel.ChangeFilterType(value);
        }

    }
}
