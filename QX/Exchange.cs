using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace QueueExchange {

    /*
     * Definition of the interfce for filters
     * Caution: Fitlers need to implement MustInitialize class and IQueueFilter
     */
    public interface IQueueFilter {
        bool Pass(string message);
    }

    public abstract class MustInitialize<XElement> {
        public MustInitialize(XElement parameters) { }
    }

    class Exchange {

        public List<Pipeline> pipes = new List<Pipeline>();    // List of configured pipes
        public List<Thread> pipeThreads = new List<Thread>();  // Reference to the thread of each pipe
        public static string configFileName;                   // Config file name "ExchangeConfig.xml" by default
        public static Dictionary<string, string> nsDict = new Dictionary<string, string>();


        public Exchange() { }

        public void Start() {

            // Set up all the Pipelines
            this.Configure();

            // Start each PipeLine in it's own thread
            foreach (Pipeline pipe in pipes) {
                // Start the pipeline in it's own thread
                Thread t = new Thread(Exchange.StartPipe);
                t.Start(pipe);
                pipeThreads.Add(t);
            }
        }

        public void Stop() {

            foreach (Pipeline p in pipes) {
                try {
                    p.StopPipeLine();
                } catch (Exception) {
                    //
                }
            }
            // Stop each of the PipeLine threads
            foreach (Thread t in pipeThreads) {
                try {
                    t.Abort();
                } catch (Exception) {
                    //
                }
            }
        }

        public void Configure() {

            // Read the configuration file and create a Pipeline for each of the defined pipes

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            Exchange.configFileName = string.IsNullOrEmpty(appSettings["ConfigFileName"]) ? "ExchangeConfig.xml" : appSettings["ConfigFileName"];

            XDocument doc = XDocument.Load(Exchange.configFileName);

            IEnumerable<XElement> nsDefn = from n in doc.Descendants("namespace") select n;

            foreach (XElement n in nsDefn) {
                nsDict.Add(n.Attribute("prefix").Value, n.Attribute("uri").Value);
            }
            IEnumerable<XElement> pipesDefn = from pipe in doc.Descendants("pipe") select pipe;

            foreach (XElement pipeConfig in pipesDefn) {
                // Create the pipeline


                int numInstances;
                try {
                    numInstances = int.Parse(pipeConfig.Attribute("numInstances").Value);
                } catch (Exception) {
                    numInstances = 1;
                }

                for (int i = 0; i < numInstances; i++) {
                    Pipeline pipe = new Pipeline(pipeConfig);
                    pipes.Add(pipe);
                }
            }
        }

        public static void StartPipe(object pipe) {
            _ = ((Pipeline)pipe).StartPipeLine();
        }
    }
}
