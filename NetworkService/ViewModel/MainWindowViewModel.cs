using NetworkService.Model;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkService.ViewModel
{
    public enum Actions { NO_ACTION, ADD, DELETE, WINDOW_CHANGED };
    public class MainWindowViewModel : BindableBase
    {
        private Entity entity;
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand UndoAction { get; set; }

        public static Object LastAction { get; set; }
        public static Actions LastActionId { get; set; }

        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();

        private BindableBase currentViewModel;

        public MainWindowViewModel()
        {
            createListener(); //Povezivanje sa serverskom aplikacijom

            NavCommand = new MyICommand<string>(OnNav);

            CurrentViewModel = networkEntitiesViewModel;

            UndoAction = new MyICommand(OnUndoAction);

            networkEntitiesViewModel.Entities.CollectionChanged += this.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Entity newEntity in e.NewItems)
                {
                    if (!networkDisplayViewModel.EntitiesInList.Contains(newEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Add(newEntity);

                        //if(newEntity.IsSolar())
                        //    networkDisplayViewModel.genSolar.EBE.Add(newEntity);
                        //else
                        //    networkDisplayViewModel.genWind.EBE.Add(newEntity);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Entity oldEntity in e.OldItems)
                {
                    if (networkDisplayViewModel.EntitiesInList.Contains(oldEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Remove(oldEntity);

                        //if (oldEntity.IsSolar())
                        //    networkDisplayViewModel.genSolar.EBE.Add(oldEntity);
                        //else
                        //    networkDisplayViewModel.genWind.EBE.Add(oldEntity);
                    }
                    else
                    {
                        int canvasIndex = networkDisplayViewModel.GetCanvasIndexForEntityId(oldEntity.Id);
                        networkDisplayViewModel.DeleteEntityFromCanvas(oldEntity);
                    }
                }
            }
        }

        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "NetworkEntities":
                    LastAction = CurrentViewModel;
                    LastActionId = Actions.WINDOW_CHANGED;
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "NetworkDisplay":
                    LastAction = CurrentViewModel;
                    LastActionId = Actions.WINDOW_CHANGED;
                    CurrentViewModel = networkDisplayViewModel;
                    break;
                case "MeasurementGraph":
                    LastAction = CurrentViewModel;
                    LastActionId = Actions.WINDOW_CHANGED;
                    CurrentViewModel = measurementGraphViewModel;
                    break;
            }
        }

        private void OnUndoAction()
        {
            if (LastAction != null && LastActionId != Actions.NO_ACTION)
            {
                //if (LastActionId == Actions.ADD)
                //{
                //    networkEntitiesViewModel.UndoAdd((Entity)LastAction);
                //}
                //if (LastActionId == Actions.DELETE)
                //{
                //    networkEntitiesViewModel.UndoDelete((Entity)LastAction);
                //}
                if (LastActionId == Actions.WINDOW_CHANGED)
                {
                    CurrentViewModel = (BindableBase)LastAction;
                }

            }


            LastAction = null;
            LastActionId = Actions.NO_ACTION;
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */

                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(networkEntitiesViewModel.Entities.Count.ToString());

                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                            string incommingEntityId = incomming.Substring(0, incomming.IndexOf(':'));
                            double newValue = double.Parse(incomming.Substring(incomming.IndexOf(':') + 1));

                            for (int idx = 0; idx < networkEntitiesViewModel.Entities.Count; idx++)
                            {
                                string currentEntityId = $"Entitet_{idx}";
                                if (currentEntityId == incommingEntityId)
                                {
                                    networkEntitiesViewModel.Entities[idx].Value = newValue;

                                    using (StreamWriter writer = File.AppendText("Log.txt"))
                                    {
                                        DateTime dateTime = DateTime.Now;
                                        writer.WriteLine($"{dateTime}: {networkEntitiesViewModel.Entities[idx].Type.Name}, {newValue}");
                                    }

                                    networkDisplayViewModel.UpdateEntityOnCanvas(networkEntitiesViewModel.Entities[idx]);
                                    //measurementGraphViewModel.OnShow();

                                    break;
                                }
                            }
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
    }
}
