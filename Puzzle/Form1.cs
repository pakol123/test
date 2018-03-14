using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using Google.Cloud.Language.V1;
using System.Net.Http;


namespace Puzzle
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen p1;
        Brush br;
        public Form1()
        {
            InitializeComponent();
            g = this.CreateGraphics();
            p1 = new Pen(Color.Red);
            br = Brushes.CadetBlue;
        }
   
        private void button1_Click(object sender, EventArgs e)
        {
           
           
          //  MessageBox.Show(g.ToString());
            this.Refresh();
            Form1 p = new Form1();
            List<string>[] listArray = new List<string>[4];
            if(!checkshape(textBox1.Text))
            {
                MessageBox.Show("This shape is not supported");
                return;
            }
            listArray =p.callURL(textBox1.Text).Result;
           
            string para = string.Join(" ", listArray[0].ToArray());
            MessageBox.Show(para);
            if (para.Contains("circle") || para.Contains("gon") || para.Contains("equilateral"))
            {
                MessageBox.Show("circle or polyogn detected");

                var b = int.Parse(listArray[0].ElementAt(listArray[2].IndexOf("NUM"))); // radius or length
                int dependencyEdge = int.Parse(listArray[1].ElementAt(listArray[2].IndexOf("NUM")));

                while (listArray[2].ElementAt(dependencyEdge) != "NOUN")
                {
                    dependencyEdge = int.Parse(listArray[1].ElementAt(dependencyEdge));
                }
                var a = listArray[0].ElementAt(dependencyEdge);
                //MessageBox.Show(a);
                //MessageBox.Show(b.ToString());

                if (para.Contains("gon"))
                {
                    int sides = 5;

                    if (para.ToLower().Contains("pent")) { sides = 5; }
                    else if (para.ToLower().Contains("hex")) { sides = 6; }
                    else if (para.ToLower().Contains("hept")) { sides = 7; }
                    else if (para.ToLower().Contains("oct")) { sides = 8; }
                    //MessageBox.Show("given sentence is polygon");
                    Point center = new Point(this.Width / 2, this.Height / 2);

                    DrawRegularPolygon(sides, b, 0, center, g);
                }
                else if (para.Contains("circle"))
                {
                   // g.DrawEllipse(p1, 50, 50, b, b);
                    g.FillEllipse(br, 50, 50, b, b);
                }
                else if (para.Contains("equilateral"))
                {
                    Point center = new Point(this.Width / 2, this.Height / 2);
                    DrawEqui(b, 25, center, g);
                }
            }
            else
            {
               // MessageBox.Show("other shape detetcted");
                int height = 0;
                int width = 0;
                string shape = "";
                if(para.ToString().ToLower().Contains("rectangle"))
                {
                    shape = "rectangle";
                }
                else if(para.ToString().ToLower().Contains("isosceles"))
                {
                    shape = "isosceles";
                }
                else if (para.ToString().ToLower().Contains("equilateral"))
                {
                    shape = "equilateral";
                }
                else if(para.ToString().ToLower().Contains("scalene"))
                {
                    shape = "scalene";
                }
                else if (para.ToString().ToLower().Contains("oval"))
                {
                    shape = "oval";
                }
                var result = Enumerable.Range(0, listArray[2].Count)
             .Where(i => listArray[2][i] == "NUM")
             .ToList();


                foreach (var item in result)
                {
                    // Console.WriteLine(item);
                    int dependencyEdge = int.Parse(listArray[1].ElementAt(item));
                    while (listArray[2].ElementAt(dependencyEdge) != "NOUN")
                    {
                        dependencyEdge = int.Parse(listArray[1].ElementAt(dependencyEdge));
                    }
                    var a = listArray[0].ElementAt(dependencyEdge);
                   // MessageBox.Show("$$$$$$"+a+"$$$$$$$");
                    if(a.ToLower() == "height" )
                    {
                        height = int.Parse(listArray[0].ElementAt(item));
                       // MessageBox.Show("height is --"+height.ToString());

                    }
                    else if(a.ToLower() == "width" || a.ToLower() == "length")
                    {
                        width = int.Parse(listArray[0].ElementAt(item));
                        //MessageBox.Show("width is --"+width.ToString());
                    }

                }
                Brush brsh = Brushes.CadetBlue;
                Pen pn = new Pen(Brushes.CadetBlue);
                switch (shape)
                {

                    case "rectangle":
                        g.DrawRectangle(p1,50,50,width,height);
                        g.FillRectangle(brsh, 50, 50, width, height);
                        break;

                    case "isosceles":
                        Drawiso(height, width, g);
                        break;

                    case "scalene":
                        Drawscalene(height, width, g);
                        break;
                    case "oval":
                        g.DrawEllipse(pn, 100, 100, width, height);
                        g.FillEllipse(brsh, 100, 100, width, height);
                        break;


                    default:
                        break;
                }
            }


           

        }
        private void DrawRegularPolygon(int sides, int radius, int startingAngle, Point center, Graphics g)
        {
            //Get the location for each vertex of the polygon
            Point[] verticies = CalculateVertices(sides, radius, startingAngle, center);

            //Render the polygon
            //Bitmap polygon = new Bitmap(this.Width, this.Height);
            using (g)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawPolygon(Pens.Black, verticies);
               // g.Dispose();
                //g = this.CreateGraphics();
            }

            //return polygon;
        }


        private Point[] CalculateVertices(int sides, int radius, int startingAngle, Point center)
        {
            if (sides < 3)
                throw new ArgumentException("Polygon must have 3 sides or more.");

            List<Point> points = new List<Point>();
            float step = 360.0f / sides;

            float angle = startingAngle; //starting angle
            for (double i = startingAngle; i < startingAngle + 360.0; i += step) //go in a full circle
            {
                points.Add(DegreesToXY(angle, radius, center)); //code snippet from above
                angle += step;
            }

            return points.ToArray();
        }

        private void Drawscalene(int height, int width, Graphics g)
        {
            Point[] vertices = CalculateScalene(height, width, g);
            Pen p = new Pen(Color.BlueViolet);
           
            g.DrawPolygon(p, vertices);
            Brush brsh = Brushes.CadetBlue;
            g.FillPolygon(brsh, vertices);

        }

        private Point[] CalculateScalene(int height, int width, Graphics g)
        {
            List<Point> points = new List<Point>();
            /*Point center = new Point(this.Width / 2, this.Height / 2);
            Point XY = new Point(center.X + 200, center.Y + 200);
            points.Add(XY);
            Point XZ= new Point(center.X + 100, center.Y + 100);
            points.Add(XZ);*/
            Point center = new Point(this.Width / 2, this.Height / 2);
            Point p1 = new Point(center.X, center.Y);
            Point p2 = new Point(center.X + width, center.Y);
            Point midpoint = new Point();
            Point p3 = new Point();
            Point p4 = new Point();


           

          
            midpoint.X = (p1.X + (width/ 4));
            midpoint.Y = p1.Y;

            p3.X = midpoint.X;
            p3.Y = midpoint.Y;

           
            Console.WriteLine("P3 is (" + p3.X + " , " + p3.Y + " )");
            p4.X = midpoint.X ;
            p4.Y = midpoint.Y+height;
            
            Console.WriteLine("P4 is (" + p4.X + " , " + p4.Y + " )");
            points.Add(p1);
            points.Add(p2);
            points.Add(p4);
            return points.ToArray();


        }

        private void Drawiso(int height, int width, Graphics g)
        {
            Point[] vertices = Calculateiso(height, width, g);
            Pen p = new Pen(Color.CadetBlue);
            
            g.DrawPolygon(p, vertices);
            Brush brsh = Brushes.CadetBlue;
            g.FillPolygon(brsh, vertices);

        }

        private Point[] Calculateiso(int height, int width, Graphics g)
        {
            List<Point> points = new List<Point>();
            
            Point center = new Point(100, 100);
            Point p1 = new Point(center.X, center.Y);
            Point p2 = new Point(center.X + width, center.Y);
            Point midpoint = new Point();
            Point p3 = new Point();
            Point p4 = new Point();

             g.DrawLine(new Pen(Brushes.Black), p1, p2);

           // double slope;

            //slope = ((double)(p2.Y - p1.Y) / (double)(p2.X - p1.X));
            //slope = -1 / slope;

            midpoint.X = (p1.X + (width/2));
            midpoint.Y = p2.Y;

            p3.X = midpoint.X;
            p3.Y = midpoint.Y;

           
            

            p4.X = midpoint.X;
            p4.Y = p3.Y + height;
            g.DrawLine(new Pen(Brushes.Blue), p3, p4);
            Console.WriteLine("P4 is (" + p4.X + " , " + p4.Y + " )");
            points.Add(p1);
            points.Add(p2);
            points.Add(p4);
            return points.ToArray();


        }

        private void DrawEqui(int radius, int startingAngle, Point center, Graphics g)
        {
            //Get the location for each vertex of the polygon
            Point[] verticies = CalculateEqui(radius, startingAngle, center);

            //Render the polygon
            //Bitmap polygon = new Bitmap(this.Width, this.Height);
            using (g)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawPolygon(Pens.Black, verticies);
            }

            //return polygon;
        }
        private Point[] CalculateEqui(int radius, int startingAngle, Point center)
        {


            List<Point> points = new List<Point>();
            float step = 360.0f / 3;

            float angle = startingAngle; //starting angle
            for (double i = startingAngle; i < startingAngle + 360.0; i += step) //go in a full circle
            {
                points.Add(DegreesToXY(angle, radius, center)); //code snippet from above
                angle += step;
            }

            return points.ToArray();
        }

        private Point DegreesToXY(float degrees, float radius, Point origin)
        {
            Point xy = new Point();
            double radians = degrees * Math.PI / 180.0;

            xy.X = (int)(Math.Cos(radians) * radius + origin.X);
            xy.Y = (int)(Math.Sin(-radians) * radius + origin.Y);
            double ddd = (Math.Sqrt(Math.Pow(Math.Abs(xy.X - origin.X), 2) + Math.Pow(Math.Abs(xy.Y - origin.Y), 2)));
            return xy;
        }

        public async Task<List<String>[]> callURL( string query)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            List<string> list1;
            List<String> list2;
            List<string> list3;
            List<string> list4;
            string url = "https://language.googleapis.com/v1beta2/documents:analyzeSyntax?key=AIzaSyAaDA7gbEIE_taX378z1RNI3sQfupeR8Tk";

            
            MessageBox.Show(query);
            string doc = "{'type':'PLAIN_TEXT','content':'"+query+"'}";
            string str = "{'document':" + doc + ",'encodingType':'UTF8'}";
            MessageBox.Show(str);
            JObject json = JObject.Parse(str);
            

            using (var client = new HttpClient())
            {
                
               
                var response = await client.PostAsJsonAsync(url, json);
                Task<string> responseString = response.Content.ReadAsStringAsync();
                string outputJson = await responseString;
                JObject o = JObject.Parse(outputJson);
                Dictionary<string, object> mydictionary = new Dictionary<string, object>();

                mydictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(o.ToString());

                var tkn = (JArray)mydictionary["tokens"];

                var jnv = tkn.ToString();

                
                List<object> mydictionary1 = new List<object>();
                mydictionary1 = JsonConvert.DeserializeObject<List<object>>(jnv.ToString());
                List<string> entWord = new List<string>();
                List<string> dependency = new List<string>();
                List<string> Tag = new List<string>();
                List<string> label = new List<string>();

                foreach (var entry in mydictionary1)
                {
                    
                    Dictionary<string, object> indict = new Dictionary<string, object>();
                    indict = JsonConvert.DeserializeObject<Dictionary<string, object>>(entry.ToString());
                   
                    Dictionary<string, string> rawtags = new Dictionary<string, string>();
                    rawtags = JsonConvert.DeserializeObject<Dictionary<string, string>>((indict["partOfSpeech"].ToString()));
                    Dictionary<string, string> rawtext = new Dictionary<string, string>();
                    rawtext = JsonConvert.DeserializeObject<Dictionary<string, string>>((indict["text"].ToString()));
                    Dictionary<string, string> rawDependency = new Dictionary<string, string>();
                    rawDependency = JsonConvert.DeserializeObject<Dictionary<string, string>>((indict["dependencyEdge"].ToString()));
                    Dictionary<string, string> rawlabel = new Dictionary<string, string>();
                    rawlabel = JsonConvert.DeserializeObject<Dictionary<string, string>>((indict["dependencyEdge"].ToString()));

                 
                    entWord.Add(rawtext["content"]);
                   
                    Tag.Add(rawtags["tag"]);
                   
                    dependency.Add((rawDependency["headTokenIndex"]));
                   
                    label.Add(rawlabel["label"]);
                }           

                list1 = entWord;
                list2 = dependency;
                list3 = Tag;
                list4 = label;
                List<String>[] output = new List<String>[4];
                output[0] = list1;
                output[1] = list2;
                output[2] = list3;
                output[3] = list4;

                return output;

            }

           
        }
       

        private Boolean checkshape(string shape)
        {
            string[] shapes = new string[] { "circle","issosceles triangle","equilateral triangle","oval","scalene triangle","circle","square","hexagon","pentagon","heptagon","octagon"};

            bool flg = false;
            foreach (var item in shapes)
            {
               
                flg = shape.Contains(item);
                if(flg == true)
                {
                    break;
                }
            }

            return flg;
            

        }

    }

}
