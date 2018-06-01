using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace PublicTransportSimulator
{
    public partial class MainWindow : Form
    {
        private ContextMenu markerMenu = new ContextMenu();
        private MenuItem command1 = null;
        private MenuItem command2 = null;
        private GMapOverlay markersOverlayStops = new GMapOverlay("Stops markers");
        private GMapOverlay markersOverlayTransport = new GMapOverlay("Transport markers");
        private GMapOverlay routes = new GMapOverlay("routes");
        private int kek = 0;
        private CancellationTokenSource cts;

        private List<BusStop> map_stops = new List<BusStop>(); //Коллекция остановок
        private List<Route> map_routes = new List<Route>(); //Коллекция маршрутов
        private List<PublicTransport> map_transport = new List<PublicTransport>(); //Коллекция транспортных средств
        List<int> lines = new List<int>(); //Список путей
        List<List<double>> lineWeigths = new List<List<double>>(); //Список весов

        private int weatherLvl = 2; //Погодные условия
        private double weatherStep = 0.05; //Начальный шаг
        private double weatherDev = 0.5; //Максимальное отклонение от выбранных уловий

        private int aveTemperture = 15; //Среднесуточная температура
        private int temperatureDev = 5; //Допустимое отклонение температуры от средней
        private double minTempWeigth = 0.005; //Минимальное влияние температуры
        private double maxTempWeigth = 0.02; //Максимальное влияние температуры

        private int timer = 0; //Счётчик времени

        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Method1(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            // access item.Tag to get the marker Tag info
        }

        private void Method2(object sender, EventArgs e)
        {
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            command1 = new MenuItem("Your command name 1", new EventHandler(Method1));
            command2 = new MenuItem("Your command name 2", new EventHandler(Method2));
            markerMenu.MenuItems.Add(command1);
            markerMenu.MenuItems.Add(command2);

            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Zoom = 5;
            gMapControl1.MaxZoom = 15;
            gMapControl1.MinZoom = 3;
            gMapControl1.MarkersEnabled = true;
            label4.Text = gMapControl1.Zoom.ToString();
            gMapControl1.OnMarkerClick += new MarkerClick(gMap_OnMarkerClick);
            using (StreamReader sR = new StreamReader("stops.txt"))
            {
                int score = 0;
                BusStop tmp = new BusStop();
                while (true)
                {
                    string temp = sR.ReadLine();
                    if (temp == null) break;
                    switch (score)
                    {
                        case 0:
                            tmp.ID = int.Parse(temp);
                            break;

                        case 1:
                            tmp.weight = int.Parse(temp);
                            break;

                        case 2:
                            tmp.name = temp;
                            break;

                        case 3:
                            tmp.adjacentIdList = space_Parsing(temp);
                            break;

                        case 4:
                            tmp.adjacentRoadsList = space_Parsing(temp);
                            break;

                        case 5:
                            tmp.routeList = space_Parsing(temp);
                            break;

                        case 6:
                            tmp.coord_X = double.Parse(temp, CultureInfo.InvariantCulture);
                            break;

                        case 7:
                            tmp.coord_Y = double.Parse(temp, CultureInfo.InvariantCulture);
                            break;
                    }
                    score += 1;
                    if (score == 8)
                    {
                        score = 0;
                        BusStop newPoint = new BusStop(tmp);
                        map_stops.Add(newPoint);
                    }
                }
            }
            for (int i = 0; i < map_stops.Count; i++)
            {
                AddPoint(map_stops[i].coord_X, map_stops[i].coord_Y);
                for (int j = 0; j < map_stops[i].adjacentIdList.Count; j++)
                {
                    bool flag = false;
                    for (int k = 0; k < lines.Count; k += 2)
                    {
                        if (lines[k] == map_stops[i].ID && lines[k + 1] == map_stops[i].adjacentIdList[j]) flag = true;
                        if (lines[k + 1] == map_stops[i].ID && lines[k] == map_stops[i].adjacentIdList[j]) flag = true;
                    }
                    if (flag == false)
                    {
                        lines.Add(map_stops[i].ID);
                        lines.Add(map_stops[i].adjacentIdList[j]);
                    }
                }
            }
            //Заполнение файла весов
            /*Random rnd = new Random();
            using (StreamWriter sw = new StreamWriter("weigths.txt", false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < lines.Count / 2; i++)
                {
                    string toFile = "";
                    for (int j = 0; j < 48; j++)
                    {
                        toFile += ((double)rnd.Next(3000, 7000) / 10000).ToString();
                        if (j != 47) toFile += " ";
                    }
                    sw.WriteLine(toFile);
                }
            }*/

            for (int i = 0; i < lines.Count; i += 2)
            {
                AddRoute(map_stops[lines[i] - 1].coord_X, map_stops[lines[i] - 1].coord_Y, map_stops[lines[i + 1] - 1].coord_X, map_stops[lines[i + 1] - 1].coord_Y);
            }
            using (StreamReader sR = new StreamReader("routes.txt"))
            {
                int score = 0;
                Route tmp = new Route();
                while (true)
                {
                    string temp = sR.ReadLine();
                    if (temp == null) break;
                    switch (score)
                    {
                        case 0:
                            tmp.ID = int.Parse(temp);
                            break;

                        case 1:
                            tmp.way = space_Parsing(temp);
                            break;
                    }
                    score += 1;
                    if (score == 2)
                    {
                        score = 0;
                        Route newRoute = new Route(tmp);
                        map_routes.Add(newRoute);
                    }
                }
            }
            using (StreamReader sR = new StreamReader("weigths.txt"))
            {
                while (true)
                {
                    string temp = sR.ReadLine();
                    if (temp == null) break;
                    List<double> tmp = new List<double>();
                    tmp = spaceDouble_Parsing(temp);
                    lineWeigths.Add(tmp);
                }
            }
            using (StreamReader sR = new StreamReader("transports.txt"))
            {
                int score = 0;
                PublicTransport tmp = new PublicTransport();
                while (true)
                {
                    string temp = sR.ReadLine();
                    if (temp == null) break;
                    switch (score)
                    {
                        case 0:
                            tmp.ID = int.Parse(temp);
                            break;

                        case 1:
                            tmp.transportId = temp;
                            break;

                        case 2:
                            tmp.transportType = temp;
                            break;

                        case 3:
                            tmp.last_stop = int.Parse(temp);
                            break;

                        case 4:
                            tmp.next_stop = int.Parse(temp);
                            break;

                        case 5:
                            tmp.progress = double.Parse(temp, CultureInfo.InvariantCulture);
                            break;
                    }
                    score += 1;
                    if (score == 6)
                    {
                        score = 0;
                        PublicTransport newTransport = new PublicTransport(tmp);
                        map_transport.Add(newTransport);
                    }
                }
            }
            for (int i = 0; i < map_transport.Count; i++)
            {
                AddTrans(map_stops[map_transport[i].last_stop - 1].coord_X, map_stops[map_transport[i].last_stop - 1].coord_Y, map_stops[map_transport[i].next_stop - 1].coord_X, map_stops[map_transport[i].next_stop - 1].coord_Y, map_transport[i].progress);
            }

            foreach (var m in markersOverlayStops.Markers)
            {
                richTextBox1.Text += m.Tag.ToString() + "\n";
            }
        }

        private void AddPoint(double latitude, double longtitude)
        {
            gMapControl1.Position = new PointLatLng(latitude, longtitude);
            //GMapOverlay markersOverlay = new GMapOverlay("markers");
            GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(latitude, longtitude), GMarkerGoogleType.green_small);
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

            marker.Tag = kek;
            kek++;
            markersOverlayStops.Markers.Add(marker);
            gMapControl1.Overlays.Add(markersOverlayStops);
        }

        private void AddRoute(double latitude1, double longtitude1, double latitude2, double longtitude2)
        {
            //GMapOverlay routes = new GMapOverlay("routes");
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(new PointLatLng(latitude1, longtitude1));
            points.Add(new PointLatLng(latitude2, longtitude2));
            gMapControl1.Position = new PointLatLng(latitude2, longtitude2);
            GMapRoute route = new GMapRoute(points, "route");
            route.Stroke = new Pen(Color.GreenYellow, 3);
            routes.Routes.Add(route);
            gMapControl1.Overlays.Add(routes);
        }

        

        private async Task DoWorkAsyncInfiniteLoop(CancellationToken token)
        {
            double i = 0;
            double speed = (double)numericUpDown1.Value * 10 / 36;
            int timePoint = 0;
            while (true)
            {
                Stopwatch sw = Stopwatch.StartNew();
                // do the work in the loop

                timeBox.Text = timer.ToString();
                timer++;
                if (timer % 1800 == 0) timePoint++;
                if (timer == (24 * 3600))
                {
                    timePoint = 0;
                    timer = 0;
                }
                int counter = 0;
                foreach (var mT in markersOverlayTransport.Markers)
                {

                    if (map_transport[counter].stay_time == 0)
                    {
                        double distance = 0;
                        for (int j = 0; j < map_stops[map_transport[counter].last_stop - 1].adjacentIdList.Count; j++)
                        {
                            if (map_transport[counter].next_stop == map_stops[map_transport[counter].last_stop - 1].adjacentIdList[j])
                            {
                                distance = map_stops[map_transport[counter].last_stop - 1].adjacentRoadsList[j];
                                break;
                            }
                        }
                        int lineNumber = 0;
                        for (int j = 0; j < lines.Count; j += 2)
                        {
                            if (lines[j] == map_transport[counter].next_stop && lines[j + 1] == map_transport[counter].last_stop)
                            {
                                lineNumber = j / 2;
                                break;
                            }
                            if (lines[j + 1] == map_transport[counter].next_stop && lines[j] == map_transport[counter].last_stop)
                            {
                                lineNumber = j / 2;
                                break;
                            }
                        }
                        Random rand = new Random();
                        double change = speed / distance * (lineWeigths[lineNumber][timePoint] - (Math.Abs(aveTemperture - 15) + rand.Next(0, temperatureDev)) * maxTempWeigth / temperatureDev - Math.Abs(weatherLvl - 2) * weatherStep * rand.Next(0, (int)(weatherDev*10000) / 10000));
                        map_transport[counter].progress += change; //пройденный процент пути
                        double lat = (map_stops[map_transport[counter].next_stop - 1].coord_X - map_stops[map_transport[counter].last_stop - 1].coord_X) * change;
                        double lng = (map_stops[map_transport[counter].next_stop - 1].coord_Y - map_stops[map_transport[counter].last_stop - 1].coord_Y) * change;
                        if (map_transport[counter].progress > 1)
                        {
                            lat -= (map_stops[map_transport[counter].next_stop - 1].coord_X - map_stops[map_transport[counter].last_stop - 1].coord_X) * (map_transport[counter].progress - 1);
                            lng -= (map_stops[map_transport[counter].next_stop - 1].coord_Y - map_stops[map_transport[counter].last_stop - 1].coord_Y) * (map_transport[counter].progress - 1);
                        }
                        mT.Position = new PointLatLng(mT.Position.Lat + lat, mT.Position.Lng + lng);
                        if (map_transport[counter].progress >= 1) //Присутствует погрешность меньше секунды
                        {
                            map_transport[counter].stay_time = (int)map_stops[map_transport[counter].next_stop - 1].weight;
                        }
                    }
                    else
                    {
                        map_transport[counter].stay_time--;
                        if (map_transport[counter].stay_time == 0)
                        {
                            map_transport[counter].progress = 0;
                            int route_num = 0;
                            for (int j = 0; j < map_routes.Count; j++)
                            {
                                if (map_transport[counter].ID == map_routes[j].ID)
                                {
             
                                    route_num = j;
                                    break;
                                }
                            }
                            int next_stage;
                            for (int j = 0; j < map_routes[route_num].way.Count; j++) //Вылетит при неверных входных данных
                            {
                                if (map_routes[route_num].way[j] == map_transport[counter].last_stop && map_routes[route_num].way[j + 1] == map_transport[counter].next_stop)
                                {
                                    if (map_routes[route_num].way[j + 1] == map_routes[route_num].way[0]) next_stage = map_routes[route_num].way[1];
                                    else next_stage = map_routes[route_num].way[j + 2];
                                    map_transport[counter].last_stop = map_transport[counter].next_stop;
                                    map_transport[counter].next_stop = next_stage;
                                    break;
                                }
                            }
                        }
                    }
                    counter++;


                }

                string newData = DateTime.Now.ToLongTimeString();
                // update the UI
                //label5.Text = "ASYNC LOOP - " + newData;
                //AddPoint(52.0975500 + i, 23.6877500);
                /*i += 0.01;
                foreach (var mT in markersOverlayTransport.Markers)
                {
                    mT.Position = new PointLatLng(mT.Position.Lat + i, mT.Position.Lng + i);
                }*/
                // don't run again for at least 200 milliseconds
                await Task.Delay(1000);
                sw.Stop();
                token.ThrowIfCancellationRequested();
                label5.Text = sw.ElapsedMilliseconds.ToString();
            }
        }

        private void gMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            object identityData = item.Tag;

            // load the menus with marker data.
            command1.Tag = identityData;
            command2.Tag = identityData;

            if (identityData != null && e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                markerMenu.Show(gMapControl1, e.Location);
            }
        }

        private List<int> space_Parsing(string space_str)
        {
            List<int> temp = new List<int>();
            List<int> result = new List<int>();
            for (int i = 0; i < space_str.Length; i++)
            {
                if (space_str[i] != ' ') temp.Add(int.Parse(space_str[i].ToString()));
                if (space_str[i] == ' ' || i == space_str.Length - 1)
                {
                    int summ = 0;
                    for (int j = temp.Count - 1, k = 1; j >= 0; j--, k *= 10)
                    {
                        summ += temp[j] * k;
                    }
                    result.Add(summ);
                    temp.RemoveRange(0, temp.Count);
                }
            }
            return result;
        }

        private List<double> spaceDouble_Parsing(string space_str)
        {
            string temp = "";
            List<double> result = new List<double>();
            for (int i = 0; i < space_str.Length; i++)
            {
                if (space_str[i] != ' ') temp += space_str[i];
                if (space_str[i] == ' ' || i == space_str.Length - 1)
                {
                    result.Add(double.Parse(temp, CultureInfo.InvariantCulture));
                    temp = "";
                }
            }
            return result;
        }
        private void AddTrans(double latitude1, double longtitude1, double latitude2, double longtitude2, double progress)
        {
            double latitude, longtitude;
            latitude = latitude1 + (latitude2 - latitude1) * progress;
            longtitude = longtitude1 + (longtitude2 - longtitude1) * progress;
            gMapControl1.Position = new PointLatLng(latitude, longtitude);
            //GMapOverlay markersOverlay = new GMapOverlay("Transport markers");
            GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(latitude, longtitude), GMarkerGoogleType.blue);
            markersOverlayTransport.Markers.Add(marker);
            gMapControl1.Overlays.Add(markersOverlayTransport);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            foreach (var mT in markersOverlayTransport.Markers)
            {
                mT.Position = new PointLatLng(mT.Position.Lat + 0.01, mT.Position.Lng + 0.01);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
                DoWorkAsyncInfiniteLoop(cts.Token);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar2.Value;
            label4.Text = gMapControl1.Zoom.ToString();
        }

        private void weatherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WeatherControl weather = new WeatherControl();
            weather.ShowDialog();
        }
    }
}