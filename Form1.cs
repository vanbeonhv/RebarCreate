using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.Operations;

using TSM = Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Tekla.Structures.ModelInternal;
using Tekla.Structures.Solid;
using System.Threading;
using RebarCreate.Functions;

namespace RebarCreate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Model myModel = new Model();
                if (myModel.GetConnectionStatus())
                {
                    Console.WriteLine("Model Connected!");
                }
                else
                {
                    Console.WriteLine("Fail to connect!");
                }
                Picker picker = new Picker();
                ModelObjectEnumerator myEnum =
                    picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);

                #region Get Center Point

                List<TSG.Point> pointList = new List<TSG.Point>();

                while (myEnum.MoveNext())
                {
                    Beam beam = myEnum.Current as Beam;
                    pointList.Add(beam.StartPoint);
                    pointList.Add(beam.EndPoint);
                }
                TSG.Point centerPoint = CreateCenterPoint(pointList);

                #endregion Get Center Point

                myEnum.Reset();
                while (myEnum.MoveNext())
                {
                    Beam beam = myEnum.Current as Beam;
                    if (beam != null)
                    {
                        #region User Input

                        string topBarRadius = "13";
                        string botBarRadius = "16";
                        string stirrupRadius = "8";
                        ArrayList numOfRebar2 = new ArrayList() { 2.0 };
                        ArrayList numOfRebar3 = new ArrayList() { 3.0 };
                        ArrayList rebarSpacing = new ArrayList() { 100.0 };

                        #endregion User Input

                        #region Workplane

                        WorkPlaneHandler workPlane =
                            myModel.GetWorkPlaneHandler();

                        TransformationPlane currentPlane =
                            workPlane.GetCurrentTransformationPlane();
                        TransformationPlane localPlane =
                            new TransformationPlane(beam.GetCoordinateSystem());

                        #endregion Workplane

                        workPlane.SetCurrentTransformationPlane(currentPlane);

                        #region Detect Orientation

                        List<double> beamPointList = new List<double>();

                        string beamOriented = null;
                        double gapX = beam.StartPoint.X - beam.EndPoint.X;
                        double gapY = beam.StartPoint.Y - beam.EndPoint.Y;
                        if (gapY >= -1 && gapY <= 1)
                        {
                            beamOriented = "hor";
                            //beamPointList.Add()
                        }
                        else if (gapX >= -1 && gapX <= 1)
                        {
                            beamOriented = "ver";
                        }
                        else
                        {
                            MessageBox.Show("Fail to handle beam orientaion!");
                        }

                        #endregion Detect Orientation

                        //MessageBox.Show(beamOriented);

                        #region Detect Position

                        string beamPosition = null;
                        switch (beamOriented)
                        {
                            case "hor":
                                if (beam.StartPoint.Y > centerPoint.Y)
                                {
                                    beamPosition = "top";
                                }
                                else if (beam.StartPoint.Y < centerPoint.Y)
                                {
                                    beamPosition = "bot";
                                }
                                else
                                {
                                    MessageBox.Show("Fail to detect position!");
                                }
                                break;

                            case "ver":
                                if (beam.StartPoint.X > centerPoint.X)
                                {
                                    beamPosition = "right";
                                }
                                else if (beam.StartPoint.X < centerPoint.X)
                                {
                                    beamPosition = "left";
                                }
                                else
                                {
                                    MessageBox.Show("Fail to detect position!");
                                }
                                break;
                        }
                        Console.WriteLine(beamPosition);

                        #endregion Detect Position

                        workPlane.SetCurrentTransformationPlane(localPlane);

                        #region Solid

                        Solid solid = beam.GetSolid();

                        double MinX = solid.MinimumPoint.X;
                        double MinY = solid.MinimumPoint.Y;
                        double MinZ = solid.MinimumPoint.Z;
                        double MaxX = solid.MaximumPoint.X;
                        double MaxY = solid.MaximumPoint.Y;
                        double MaxZ = solid.MaximumPoint.Z;
                        workPlane.SetCurrentTransformationPlane(currentPlane);

                        ArrayList currentBeam = new ArrayList() { beam };
                        TSMUI.ModelObjectSelector selector = new TSMUI.ModelObjectSelector();
                        selector.Select(currentBeam, true);

                        FaceEnumerator faceEnum = solid.GetFaceEnumerator();
                        List<TSG.Point> facePointsList = new List<TSG.Point>();
                        while (faceEnum.MoveNext())
                        {
                            Face face = (Face)faceEnum.Current;
                            switch (beamPosition)
                            {
                                case "right":
                                    if (face.Normal == new Vector(1.0, 0.0, 0.0))
                                    {
                                        LoopEnumerator loopEnum = face.GetLoopEnumerator();
                                        while (loopEnum.MoveNext())
                                        {
                                            Loop loop = (Loop)loopEnum.Current;
                                            if (loop != null)
                                            {
                                                VertexEnumerator vertexEnum = loop.GetVertexEnumerator();
                                                while (vertexEnum.MoveNext())
                                                {
                                                    TSG.Point vertex = (TSG.Point)vertexEnum.Current;
                                                    facePointsList.Add(vertex);
                                                    //Insert Points for ez visulize
                                                    //ControlPoint controlPoint = new ControlPoint(vertex);
                                                    //controlPoint.Insert();
                                                    myModel.CommitChanges();
                                                    //Thread.Sleep(100);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    };

                                    break;

                                case "left":
                                    if (face.Normal == new Vector(-1.0, 0.0, 0.0))
                                    {
                                        LoopEnumerator loopEnum = face.GetLoopEnumerator();
                                        while (loopEnum.MoveNext())
                                        {
                                            Loop loop = (Loop)loopEnum.Current;
                                            if (loop != null)
                                            {
                                                VertexEnumerator vertexEnum = loop.GetVertexEnumerator();
                                                while (vertexEnum.MoveNext())
                                                {
                                                    TSG.Point vertex = (TSG.Point)vertexEnum.Current;
                                                    facePointsList.Add(vertex);
                                                    //ControlPoint controlPoint = new ControlPoint(vertex);
                                                    //controlPoint.Insert();
                                                    myModel.CommitChanges();
                                                    Thread.Sleep(100);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    };
                                    break;

                                case "bot":
                                    if (face.Normal == new Vector(0.0, -1.0, 0.0))
                                    {
                                        LoopEnumerator loopEnum = face.GetLoopEnumerator();
                                        while (loopEnum.MoveNext())
                                        {
                                            Loop loop = (Loop)loopEnum.Current;
                                            if (loop != null)
                                            {
                                                VertexEnumerator vertexEnum = loop.GetVertexEnumerator();
                                                while (vertexEnum.MoveNext())
                                                {
                                                    TSG.Point vertex = (TSG.Point)vertexEnum.Current;
                                                    facePointsList.Add(vertex);
                                                    //ControlPoint controlPoint = new ControlPoint(vertex);
                                                    //controlPoint.Insert();
                                                    myModel.CommitChanges();
                                                    Thread.Sleep(100);
                                                }
                                            }
                                        }
                                        Console.WriteLine(facePointsList[0]);
                                    }
                                    else
                                    {
                                        break;
                                    };
                                    break;

                                case "top":
                                    if (face.Normal == new Vector(0.0, 1.0, 0.0))
                                    {
                                        LoopEnumerator loopEnum = face.GetLoopEnumerator();
                                        while (loopEnum.MoveNext())
                                        {
                                            Loop loop = (Loop)loopEnum.Current;
                                            if (loop != null)
                                            {
                                                VertexEnumerator vertexEnum = loop.GetVertexEnumerator();
                                                while (vertexEnum.MoveNext())
                                                {
                                                    TSG.Point vertex = (TSG.Point)vertexEnum.Current;
                                                    facePointsList.Add(vertex);
                                                    //ControlPoint controlPoint = new ControlPoint(vertex);
                                                    //controlPoint.Insert();
                                                    myModel.CommitChanges();
                                                    Thread.Sleep(100);
                                                }
                                            }
                                        }
                                        Console.WriteLine(facePointsList[0]);
                                    }
                                    else
                                    {
                                        break;
                                    };
                                    break;
                            }
                        }
                        //Check drop
                        string dropPosition = null;
                        if (facePointsList.Count > 4)
                        {
                            PointFilter pointFilter = new PointFilter(facePointsList);
                            if (beamPosition == "left" || beamPosition == "right")
                            {
                                double maxY = pointFilter.MaxY();
                                double minY = pointFilter.MinY();
                                //Cac part can nam trong cung 1 goc phan tu
                                double gapToMaxY = Math.Abs(facePointsList[0].Y - maxY);
                                double gapToMinY = Math.Abs(facePointsList[0].Y - minY);
                                if (gapToMaxY < gapToMinY)
                                {
                                    dropPosition = "top";
                                }
                                else
                                {
                                    dropPosition = "bot";
                                }
                            }
                            else if (beamPosition == "top" || beamPosition == "bot")
                            {
                                double maxX = pointFilter.MaxX();
                                double minX = pointFilter.MinX();
                                double gapToMaxX = Math.Abs(facePointsList[0].X - maxX);
                                double gapToMinX = Math.Abs(facePointsList[0].X - minX);
                                if (gapToMaxX < gapToMinX)
                                {
                                    dropPosition = "right";
                                }
                                else
                                {
                                    dropPosition = "left";
                                }
                            }
                            Console.WriteLine(dropPosition);
                            CreateTopRebar(beam, dropPosition, facePointsList, topBarRadius, beamOriented, beamPosition, numOfRebar2);
                            CreateStirrupDrop(beam, dropPosition, facePointsList, stirrupRadius, beamOriented, beamPosition, rebarSpacing);
                            workPlane.SetCurrentTransformationPlane(localPlane);
                            CreateBotRebar(beam, dropPosition, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, botBarRadius, beamOriented, beamPosition, numOfRebar2);
                            workPlane.SetCurrentTransformationPlane(currentPlane);
                        }
                        else
                        {
                            Console.WriteLine("test");
                            workPlane.SetCurrentTransformationPlane(localPlane);
                            CreateTopRebar(beam, dropPosition, MinX, MinZ, MaxX, MinY, MaxY, MaxZ, topBarRadius, beamOriented, beamPosition, numOfRebar2);
                            CreateBotRebar(beam, dropPosition, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, botBarRadius, beamOriented, beamPosition, numOfRebar2);
                            CreateStirrup(beam, dropPosition, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, stirrupRadius, beamOriented, beamPosition, rebarSpacing);
                            workPlane.SetCurrentTransformationPlane(currentPlane);
                        }

                        #endregion Solid

                        workPlane.SetCurrentTransformationPlane(localPlane);

                        //CreateTopRebar(beam, dropPosition, MinX, MinZ, MaxX, MinY, MaxY, MaxZ, "13", beamOriented, beamPosition, numOfRebar2);
                        //CreateBotRebar(beam, dropPosition, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, "16", beamOriented, beamPosition, numOfRebar2);
                        //CreateStirrup(beam, dropPosition, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, "8", beamOriented, beamPosition, rebarSpacing);

#warning    Reset working plan
                        workPlane.SetCurrentTransformationPlane(currentPlane);
                        myModel.CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static TSG.Point CreateCenterPoint(List<TSG.Point> pointList)
        {
            double XPoint = 0;
            double YPoint = 0;
            pointList.ForEach(point =>
            {
                XPoint += point.X;
                YPoint += point.Y;
            });
            TSG.Point centerPoint =
                new TSG.Point(XPoint / pointList.Count, YPoint / pointList.Count);

            ControlPoint controlPoint = new ControlPoint(centerPoint);
            controlPoint.Insert();
            return centerPoint;
        }

        private void CreateBotRebar(Beam beam, string dropPosition, double MinX, double MaxY, double MinY, double MinZ, double MaxX, double MaxZ, string radius, string beamOriented, string beamPosition, ArrayList numOfRebar)
        {
            #region Bot rebar

            TSG.Point pt1 = new TSG.Point(MinX, MaxY - 46, MinZ);
            TSG.Point pt2 = new TSG.Point(MinX, MaxY - 46, MaxZ);
            TSG.Point pt3 = new TSG.Point(MaxX, MaxY - 46, MaxZ);
            TSG.Point pt4 = new TSG.Point(MaxX, MaxY - 46, MinZ);
            //
            TSG.Point pb1 = new TSG.Point(MinX, MinY + 46, MinZ);
            TSG.Point pb2 = new TSG.Point(MinX, MinY + 46, MaxZ);
            TSG.Point pb3 = new TSG.Point(MaxX, MinY + 46, MaxZ);
            TSG.Point pb4 = new TSG.Point(MaxX, MinY + 46, MinZ);
            Polygon RebarPolygonBot = new Polygon();

            TSG.Point spBot = null;
            TSG.Point epBot = null;
            switch (beamPosition)
            {
                case "top":
                    RebarPolygonBot.Points.Add(pt1);
                    RebarPolygonBot.Points.Add(pt2);
                    RebarPolygonBot.Points.Add(pt3);
                    RebarPolygonBot.Points.Add(pt4);
                    spBot = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MaxZ);
                    epBot = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MinZ);
                    break;

                case "bot":
                    RebarPolygonBot.Points.Add(pt2);
                    RebarPolygonBot.Points.Add(pt1);
                    RebarPolygonBot.Points.Add(pt4);
                    RebarPolygonBot.Points.Add(pt3);
                    spBot = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MinZ);
                    epBot = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MaxZ);
                    break;

                case "right":
                    RebarPolygonBot.Points.Add(pb1);
                    RebarPolygonBot.Points.Add(pb2);
                    RebarPolygonBot.Points.Add(pb3);
                    RebarPolygonBot.Points.Add(pb4);
                    spBot = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MaxZ);
                    epBot = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MinZ);
                    break;

                case "left":
                    RebarPolygonBot.Points.Add(pb2);
                    RebarPolygonBot.Points.Add(pb1);
                    RebarPolygonBot.Points.Add(pb4);
                    RebarPolygonBot.Points.Add(pb3);
                    spBot = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MinZ);
                    epBot = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MaxZ);
                    break;

                default:
                    break;
            }

            InsertRebarInfo(spBot, epBot, RebarPolygonBot, beam, radius, "BOT BAR", beamOriented, numOfRebar, dropPosition);

            #endregion Bot rebar
        }

        private void CreateTopRebar(Beam beam, string dropPosition, List<TSG.Point> facePointsList, string radius, string beamOriented, string beamPosition, ArrayList numOfRebar)
        {
            #region Top rebar

            string rebarName = "TOP BAR_H" + radius;
            string beamProfile = null;
            beam.GetReportProperty("PROFILE", ref beamProfile);
            double beamWidth = Convert.ToDouble(beamProfile.Substring(0, beamProfile.IndexOf("x")));
            Polygon RebarPolygonTop = new Polygon();
            double dropDepth;
            TSG.Point p1 = null;
            TSG.Point p2 = null;
            TSG.Point p3 = null;
            TSG.Point p4 = null;

            TSG.Point FPL0 = facePointsList[0];
            TSG.Point FPL1 = facePointsList[1];
            TSG.Point FPL2 = facePointsList[2];
            TSG.Point FPL3 = facePointsList[3];
            TSG.Point FPL4 = facePointsList[4];
            TSG.Point FPL5 = facePointsList[5];

            TSG.Point spTop = null;
            TSG.Point epTop = null;
            switch ((beamPosition))
            {
                case "right" when dropPosition == "top":
                    dropDepth = Math.Abs(FPL5.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL4.X - beamWidth, FPL4.Y, FPL4.Z);
                    p2 = new TSG.Point(FPL5.X, FPL5.Y - 50.0 - dropDepth * 3, FPL5.Z);
                    p3 = new TSG.Point(FPL0.X, FPL0.Y - 50.0, FPL0.Z);
                    p4 = new TSG.Point(FPL1.X - beamWidth, FPL1.Y, FPL1.Z);
                    RebarPolygonTop.Points.Add(p1);
                    RebarPolygonTop.Points.Add(FPL4);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(FPL1);
                    RebarPolygonTop.Points.Add(p4);
                    spTop = new TSG.Point(FPL4.X, (FPL4.Y + FPL1.Y) / 2, FPL4.Z);
                    epTop = new TSG.Point(FPL4.X - beamWidth, (FPL4.Y + FPL1.Y) / 2, FPL4.Z);
                    break;

                case "right" when dropPosition == "bot":
                    dropDepth = Math.Abs(FPL1.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL5.X - beamWidth, FPL5.Y, FPL5.Z);
                    p2 = new TSG.Point(FPL0.X, FPL0.Y + 50.0, FPL0.Z);
                    p3 = new TSG.Point(FPL1.X, FPL1.Y + 50.0 + dropDepth * 3, FPL1.Z);
                    p4 = new TSG.Point(FPL2.X - beamWidth, FPL2.Y, FPL2.Z);

                    RebarPolygonTop.Points.Add(p4);
                    RebarPolygonTop.Points.Add(FPL2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(FPL5);
                    RebarPolygonTop.Points.Add(p1);
                    spTop = new TSG.Point(FPL2.X, (FPL2.Y + FPL5.Y) / 2, FPL2.Z);
                    epTop = new TSG.Point(FPL2.X - beamWidth, (FPL2.Y + FPL5.Y) / 2, FPL2.Z);
                    break;

                case "left" when dropPosition == "top":
                    dropDepth = Math.Abs(FPL2.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL0.X + beamWidth, FPL0.Y, FPL0.Z);
                    p2 = new TSG.Point(FPL1.X, FPL1.Y - 50.0, FPL1.Z);
                    p3 = new TSG.Point(FPL2.X, FPL2.Y - 50.0 - dropDepth * 3, FPL2.Z);
                    p4 = new TSG.Point(FPL3.X + beamWidth, FPL3.Y, FPL3.Z);

                    RebarPolygonTop.Points.Add(p4);
                    RebarPolygonTop.Points.Add(FPL3);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(FPL0);
                    RebarPolygonTop.Points.Add(p1);
                    spTop = new TSG.Point(FPL3.X, (FPL0.Y + FPL3.Y) / 2, FPL3.Z);
                    epTop = new TSG.Point(FPL3.X + beamWidth, (FPL0.Y + FPL3.Y) / 2, FPL3.Z);
                    break;

                case "left" when dropPosition == "bot":
                    dropDepth = Math.Abs(FPL0.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL5.X + beamWidth, FPL5.Y, FPL5.Z);
                    p2 = new TSG.Point(FPL0.X, FPL0.Y + 50.0 + dropDepth * 3, FPL0.Z);
                    p3 = new TSG.Point(FPL1.X, FPL1.Y + 50.0, FPL1.Z);
                    p4 = new TSG.Point(FPL2.X + beamWidth, FPL2.Y, FPL2.Z);

                    RebarPolygonTop.Points.Add(p1);
                    RebarPolygonTop.Points.Add(FPL5);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(FPL2);
                    RebarPolygonTop.Points.Add(p4);
                    spTop = new TSG.Point(FPL5.X, (FPL2.Y + FPL5.Y) / 2, FPL5.Z);
                    epTop = new TSG.Point(FPL5.X + beamWidth, (FPL2.Y + FPL5.Y) / 2, FPL5.Z);
                    break;

                case "bot" when dropPosition == "left":
                    dropDepth = Math.Abs(FPL2.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL3.X, FPL3.Y + beamWidth, FPL3.Z);
                    p2 = new TSG.Point(FPL2.X + 50.0 + dropDepth * 3, FPL2.Y, FPL2.Z);
                    p3 = new TSG.Point(FPL1.X + 50.0, FPL1.Y, FPL1.Z);
                    p4 = new TSG.Point(FPL0.X, FPL0.Y + beamWidth, FPL0.Z);

                    spTop = new TSG.Point((FPL0.X + FPL3.X) / 2, FPL3.Y, FPL3.Z);
                    epTop = new TSG.Point((FPL0.X + FPL3.X) / 2, FPL3.Y + beamWidth, FPL3.Z);

                    RebarPolygonTop.Points.Add(p1);
                    RebarPolygonTop.Points.Add(FPL3);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(FPL0);
                    RebarPolygonTop.Points.Add(p4);

                    break;

                case "bot" when dropPosition == "right":
                    dropDepth = Math.Abs(FPL5.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL4.X, FPL4.Y + beamWidth, FPL4.Z);
                    p2 = new TSG.Point(FPL5.X - 50.0 - dropDepth * 3, FPL5.Y, FPL5.Z);
                    p3 = new TSG.Point(FPL0.X - 50.0, FPL0.Y, FPL0.Z);
                    p4 = new TSG.Point(FPL1.X, FPL1.Y + beamWidth, FPL1.Z);

                    RebarPolygonTop.Points.Add(p4);
                    RebarPolygonTop.Points.Add(FPL1);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(FPL4);
                    RebarPolygonTop.Points.Add(p1);

                    spTop = new TSG.Point((FPL1.X + FPL4.X) / 2, FPL4.Y, FPL4.Z);
                    epTop = new TSG.Point((FPL1.X + FPL4.X) / 2, FPL4.Y + beamWidth, FPL4.Z);

                    break;

                case "top" when dropPosition == "left":
                    dropDepth = Math.Abs(FPL0.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL5.X, FPL5.Y - beamWidth, FPL5.Z);
                    p2 = new TSG.Point(FPL0.X + 50.0 + dropDepth * 3, FPL0.Y, FPL0.Z);
                    p3 = new TSG.Point(FPL1.X + 50.0, FPL1.Y, FPL1.Z);
                    p4 = new TSG.Point(FPL2.X, FPL2.Y - beamWidth, FPL2.Z);

                    RebarPolygonTop.Points.Add(p1);
                    RebarPolygonTop.Points.Add(FPL5);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(FPL2);
                    RebarPolygonTop.Points.Add(p4);

                    spTop = new TSG.Point((FPL2.X + FPL5.X) / 2, FPL5.Y, FPL5.Z);
                    epTop = new TSG.Point((FPL2.X + FPL5.X) / 2, FPL5.Y - beamWidth, FPL5.Z);

                    break;

                case "top" when dropPosition == "right":
                    dropDepth = Math.Abs(FPL2.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL3.X, FPL3.Y - beamWidth, FPL3.Z);
                    p2 = new TSG.Point(FPL2.X - 50.0 - dropDepth * 3, FPL2.Y, FPL2.Z);
                    p3 = new TSG.Point(FPL1.X - 50.0, FPL1.Y, FPL1.Z);
                    p4 = new TSG.Point(FPL0.X, FPL0.Y - beamWidth, FPL0.Z);
#warning Tekla draw rebar loi
                    RebarPolygonTop.Points.Add(p1);
                    RebarPolygonTop.Points.Add(FPL3);
                    RebarPolygonTop.Points.Add(p2);
                    RebarPolygonTop.Points.Add(p3);
                    RebarPolygonTop.Points.Add(FPL0);
                    RebarPolygonTop.Points.Add(p4);

                    spTop = new TSG.Point((FPL0.X + FPL3.X) / 2, FPL3.Y, FPL3.Z);
                    epTop = new TSG.Point((FPL0.X + FPL3.X) / 2, FPL3.Y - beamWidth, FPL3.Z);

                    break;
            }

            InsertRebarInfo(spTop, epTop, RebarPolygonTop, beam, radius, rebarName, beamOriented, numOfRebar, dropPosition);

            #endregion Top rebar
        }

        private void CreateTopRebar(Beam beam, string dropPosition, double MinX, double MinZ, double MaxX, double MinY, double MaxY, double MaxZ, string radius, string beamOriented, string beamPosition, ArrayList numOfRebar)
        {
            #region Top rebar first

            TSG.Point pb1 = new TSG.Point(MinX, MinY + 46, MinZ);
            TSG.Point pb2 = new TSG.Point(MinX, MinY + 46, MaxZ);
            TSG.Point pb3 = new TSG.Point(MaxX, MinY + 46, MaxZ);
            TSG.Point pb4 = new TSG.Point(MaxX, MinY + 46, MinZ);
            //
            TSG.Point pt1 = new TSG.Point(MinX, MaxY - 46, MinZ);
            TSG.Point pt2 = new TSG.Point(MinX, MaxY - 46, MaxZ);
            TSG.Point pt3 = new TSG.Point(MaxX, MaxY - 46, MaxZ);
            TSG.Point pt4 = new TSG.Point(MaxX, MaxY - 46, MinZ);
            TSG.Point spTop = null;
            TSG.Point epTop = null;
            Polygon RebarPolygonTop = new Polygon();
            switch (beamPosition)
            {
                case "top":
                    RebarPolygonTop.Points.Add(pb1);
                    RebarPolygonTop.Points.Add(pb2);
                    RebarPolygonTop.Points.Add(pb3);
                    RebarPolygonTop.Points.Add(pb4);
                    spTop = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MaxZ);
                    epTop = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MinZ);
                    break;

                case "bot":
                    RebarPolygonTop.Points.Add(pb2);
                    RebarPolygonTop.Points.Add(pb1);
                    RebarPolygonTop.Points.Add(pb4);
                    RebarPolygonTop.Points.Add(pb3);
                    spTop = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MinZ);
                    epTop = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MaxZ);
                    break;

                case "right":
                    RebarPolygonTop.Points.Add(pt1);
                    RebarPolygonTop.Points.Add(pt2);
                    RebarPolygonTop.Points.Add(pt3);
                    RebarPolygonTop.Points.Add(pt4);
                    spTop = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MaxZ);
                    epTop = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MinZ);
                    break;

                case "left":
                    RebarPolygonTop.Points.Add(pt2);
                    RebarPolygonTop.Points.Add(pt1);
                    RebarPolygonTop.Points.Add(pt4);
                    RebarPolygonTop.Points.Add(pt3);
                    spTop = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MinZ);
                    epTop = new TSG.Point((MinX + MaxX) / 2, MaxY - 46, MaxZ);
                    break;
            }
            InsertRebarInfo(spTop, epTop, RebarPolygonTop, beam, radius, "TOP BAR", beamOriented, numOfRebar, dropPosition);

            #endregion Top rebar first
        }

        public void CreateCol(TSG.Point sp, TSG.Point ep)
        {
            Beam myBeam = new Beam(Beam.BeamTypeEnum.COLUMN)
            {
                StartPoint = sp,
                EndPoint = ep
            };
            myBeam.Profile.ProfileString = "400*400";
            myBeam.Insert();
            if (myBeam.Insert())
            {
                MessageBox.Show("success!");
            }
        }

        public void InsertRebarInfo(TSG.Point sp, TSG.Point ep, Polygon RebarPolygon, Beam beam, string radius, string rebarName, string beamOriented, ArrayList numOrSpacingOfRebar, string dropPosition)
        {
            double radiusDouble = Convert.ToDouble(radius);
            RebarGroup rebar = new RebarGroup()
            {
                StartPoint = sp,
                EndPoint = ep,
                Name = rebarName,
                Grade = Convert.ToString('H'),
                Class = 2,
                Size = radius,
            };
            rebar.Polygons.Add(RebarPolygon);

            if (dropPosition == null)
            {
                if (rebarName.Contains("TOP BAR") || rebarName.Contains("BOT BAR"))
                {
                    BeamMainbarInfo(beam, beamOriented, radiusDouble, rebar, numOrSpacingOfRebar);
                }
                else if (rebarName.Contains("STIRRUP BAR"))
                {
                    BeamStirrupBarInfo(beam, beamOriented, radiusDouble, rebar, numOrSpacingOfRebar, dropPosition);
                }
                else
                {
                    Console.WriteLine("Rebar name error!");
                }
            }
            else if (dropPosition != null)
            {
                if (rebarName.Contains("TOP BAR"))
                {
                    TopBarInfo(beam, beamOriented, radiusDouble, rebar, numOrSpacingOfRebar);
                }
                else if (rebarName.Contains("BOT BAR"))
                {
                    BeamMainbarInfo(beam, beamOriented, radiusDouble, rebar, numOrSpacingOfRebar);
                }
                else if (rebarName.Contains("STIRRUP BAR"))
                {
                    BeamStirrupBarInfo(beam, beamOriented, radiusDouble, rebar, numOrSpacingOfRebar, dropPosition);
                }
                else
                {
                    Console.WriteLine("Rebar name error!");
                }
            }

            //if (rebar.Insert())
            //{
            //    MessageBox.Show("Success!");
            //}
        }

        private static void TopBarInfo(Beam beam, string beamOriented, double radiusDouble, RebarGroup rebar, ArrayList numOfRebar)
        {
            rebar.RadiusValues.Add(2 * radiusDouble);
            rebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
            rebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;

            rebar.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_NUMBER;

            rebar.Spacings = numOfRebar;
            rebar.StartPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            rebar.EndPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            if (radiusDouble == 16.0)
            {
                rebar.StartPointOffsetValue = 390;
                rebar.EndPointOffsetValue = 390;
            }
            else if (radiusDouble == 13.0)
            {
                rebar.StartPointOffsetValue = 285;
                rebar.EndPointOffsetValue = 285;
            }

            //PlanOffsets
#warning Can check lai cong thuc
            ArrayList onPlanOffsets = null;
            if (beamOriented == "hor")
            {
                onPlanOffsets = new ArrayList
            {
                (200 - 38 - radiusDouble) * -1,
                0.0, 0.0, 0.0,
                (200 - 38 - radiusDouble) * -1
            };
            }
            else if (beamOriented == "ver")
            {
                onPlanOffsets = new ArrayList
            {
                (38 + radiusDouble),
                0.0, 0.0, 0.0,
                (38 + radiusDouble)
            };
            };
            if (onPlanOffsets == null)
            {
                MessageBox.Show("false");
            }
            rebar.OnPlaneOffsets = onPlanOffsets;

            rebar.ExcludeType = RebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
            rebar.Father = beam;

            //Check if need radius or not??? Could be only 38/28
            rebar.StartFromPlaneOffset = 28 + radiusDouble / 2;
            rebar.EndFromPlaneOffset = 38 + radiusDouble / 2;
            rebar.Insert();
            TSM.Operations.Operation.MoveObject(rebar, new Vector(0, 0, -(39 + radiusDouble)));
        }

        private static void BeamMainbarInfo(Beam beam, string beamOriented, double radiusDouble, RebarGroup rebar, ArrayList numOfRebar)
        {
            rebar.RadiusValues.Add(2 * radiusDouble);
            rebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
            rebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;

            rebar.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_NUMBER;

            rebar.Spacings = numOfRebar;
            rebar.StartPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            rebar.EndPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            if (radiusDouble == 16.0)
            {
                rebar.StartPointOffsetValue = 390;
                rebar.EndPointOffsetValue = 390;
            }
            else if (radiusDouble == 13.0)
            {
                rebar.StartPointOffsetValue = 285;
                rebar.EndPointOffsetValue = 285;
            }

            //PlanOffsets
#warning Can chack lai cong thuc
            ArrayList onPlanOffsets = null;
            if (beamOriented == "hor")
            {
                onPlanOffsets = new ArrayList
            {
                (200 - 38 - radiusDouble) * -1,
                0.0,
                (200 - 38 - radiusDouble) * -1
            };
            }
            else if (beamOriented == "ver")
            {
                onPlanOffsets = new ArrayList
            {
                (38 + radiusDouble),
                0.0,
                (38 + radiusDouble)
            };
            };
            if (onPlanOffsets == null)
            {
                MessageBox.Show("false");
            }
            rebar.OnPlaneOffsets = onPlanOffsets;

            rebar.ExcludeType = RebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
            rebar.Father = beam;

            //Check if need radius or not??? Could be only 38/28
            rebar.StartFromPlaneOffset = 28 + radiusDouble / 2;
            rebar.EndFromPlaneOffset = 38 + radiusDouble / 2;
            rebar.Insert();
        }

        public void CreateStirrup(Beam beam, string dropPosition, double MinX, double MaxY, double MinY, double MinZ, double MaxX, double MaxZ, string radius, string beamOriented, string beamPosition, ArrayList rebarSpacing)
        {
            TSG.Point p1 = new TSG.Point(MinX, MinY, MinZ);
            TSG.Point p2 = new TSG.Point(MinX, MinY, MaxZ);
            TSG.Point p3 = new TSG.Point(MinX, MaxY, MaxZ);
            TSG.Point p4 = new TSG.Point(MinX, MaxY, MinZ);

            TSG.Point sp = null;
            TSG.Point ep = null;
            //TSG.Point sp = beam.GetCenterLine(true)[0] as TSG.Point;
            //TSG.Point ep = beam.GetCenterLine(true)[1] as TSG.Point;
            Polygon RebarPolygon = new Polygon();
            switch (beamOriented)
            {
                case "hor":
                    RebarPolygon.Points.Add(p1);
                    RebarPolygon.Points.Add(p2);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(p1);
                    sp = new TSG.Point(MinX, MinY, (MinZ + MaxZ) / 2);
                    ep = new TSG.Point(MaxX, MinY, (MinZ + MaxZ) / 2);
                    break;

                case "ver":
                    RebarPolygon.Points.Add(p1);
                    RebarPolygon.Points.Add(p2);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(p1);
                    sp = new TSG.Point(MinX, MaxY, (MinZ + MaxZ) / 2);
                    ep = new TSG.Point(MaxX, MaxY, (MinZ + MaxZ) / 2);
                    break;
            }
            InsertRebarInfo(sp, ep, RebarPolygon, beam, radius, "STIRRUP BAR", beamOriented, rebarSpacing, dropPosition);
        }

        private void CreateStirrupDrop(Beam beam, string dropPosition, List<TSG.Point> facePointsList, string radius, string beamOriented, string beamPosition, ArrayList rebarSpacing)
        {
            string rebarName = "STIRRUP BAR";
            string beamProfile = null;
            beam.GetReportProperty("PROFILE", ref beamProfile);
            double beamWidth = Convert.ToDouble(beamProfile.Substring(0, beamProfile.IndexOf("x")));
            Polygon RebarPolygon = new Polygon();
            Polygon RebarPolygonDrop = new Polygon();
            double dropDepth;
            TSG.Point p1 = null;
            TSG.Point p2 = null;
            TSG.Point p3 = null;
            TSG.Point p4 = null;

            TSG.Point FPL0 = facePointsList[0];
            TSG.Point FPL1 = facePointsList[1];
            TSG.Point FPL2 = facePointsList[2];
            TSG.Point FPL3 = facePointsList[3];
            TSG.Point FPL4 = facePointsList[4];
            TSG.Point FPL5 = facePointsList[5];

            TSG.Point spDrop = null;
            TSG.Point epDrop = null;
            TSG.Point spMain = null;
            TSG.Point epMain = null;
            switch ((beamPosition))
            {
                case "right" when dropPosition == "top":
                    //dropDepth = Math.Abs(FPL5.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL2.X - beamWidth, FPL2.Y, FPL2.Z);
                    p2 = new TSG.Point(FPL1.X - beamWidth, FPL1.Y, FPL1.Z);
                    RebarPolygonDrop.Points.Add(FPL1);
                    RebarPolygonDrop.Points.Add(FPL2);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(FPL1);

                    spDrop = new TSG.Point((FPL1.X + p1.X) / 2, FPL1.Y - 200.0, FPL1.Z);
                    epDrop = new TSG.Point((FPL1.X + p1.X) / 2, FPL0.Y, FPL0.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL3.X - beamWidth, FPL3.Y, FPL3.Z);
                    p4 = new TSG.Point(FPL4.X - beamWidth, FPL4.Y, FPL4.Z);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(FPL3);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(FPL4);

                    spMain = new TSG.Point((FPL4.X + p3.X) / 2, FPL4.Y + 200.0, FPL4.Z);
                    epMain = new TSG.Point((FPL4.X + p3.X) / 2, FPL5.Y, FPL5.Z);
                    break;

                case "right" when dropPosition == "bot":
                    //dropDepth = Math.Abs(FPL1.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL4.X - beamWidth, FPL4.Y, FPL4.Z);
                    p2 = new TSG.Point(FPL5.X - beamWidth, FPL5.Y, FPL5.Z);
                    RebarPolygonDrop.Points.Add(FPL5);
                    RebarPolygonDrop.Points.Add(FPL4);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(FPL5);

                    spDrop = new TSG.Point((FPL5.X + p2.X) / 2, FPL5.Y + 200.0, FPL5.Z);
                    epDrop = new TSG.Point((FPL0.X + p2.X) / 2, FPL0.Y, FPL0.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL3.X - beamWidth, FPL3.Y, FPL3.Z);
                    p4 = new TSG.Point(FPL2.X - beamWidth, FPL2.Y, FPL2.Z);
                    RebarPolygon.Points.Add(FPL2);
                    RebarPolygon.Points.Add(FPL3);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(FPL2);

                    spMain = new TSG.Point((FPL2.X + p4.X) / 2, FPL2.Y - 200.0, FPL2.Z);
                    epMain = new TSG.Point((FPL2.X + p4.X) / 2, FPL1.Y, FPL1.Z);
                    break;

                case "left" when dropPosition == "top":
                    //dropDepth = Math.Abs(FPL2.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL0.X + beamWidth, FPL0.Y, FPL0.Z);
                    p2 = new TSG.Point(FPL5.X + beamWidth, FPL5.Y, FPL5.Z);
                    RebarPolygonDrop.Points.Add(FPL0);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(FPL5);
                    RebarPolygonDrop.Points.Add(FPL0);

                    spDrop = new TSG.Point((FPL0.X + p1.X) / 2, FPL0.Y - 200.0, FPL0.Z);
                    epDrop = new TSG.Point((FPL0.X + p1.X) / 2, FPL1.Y, FPL1.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL3.X + beamWidth, FPL3.Y, FPL3.Z);
                    p4 = new TSG.Point(FPL4.X + beamWidth, FPL4.Y, FPL4.Z);
                    RebarPolygon.Points.Add(FPL3);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(FPL3);

                    spMain = new TSG.Point((FPL3.X + p3.X) / 2, FPL3.Y + 200.0, FPL3.Z);
                    epMain = new TSG.Point((FPL3.X + p3.X) / 2, FPL2.Y, FPL2.Z);
                    break;

                case "left" when dropPosition == "bot":
                    //dropDepth = Math.Abs(FPL0.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL2.X + beamWidth, FPL2.Y, FPL2.Z);
                    p2 = new TSG.Point(FPL3.X + beamWidth, FPL3.Y, FPL3.Z);
                    RebarPolygonDrop.Points.Add(FPL2);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(FPL3);
                    RebarPolygonDrop.Points.Add(FPL2);

                    spDrop = new TSG.Point((FPL2.X + p1.X) / 2, FPL2.Y + 200.0, FPL2.Z);
                    epDrop = new TSG.Point((FPL0.X + p1.X) / 2, FPL1.Y, FPL1.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL5.X + beamWidth, FPL5.Y, FPL5.Z);
                    p4 = new TSG.Point(FPL4.X + beamWidth, FPL4.Y, FPL4.Z);
                    RebarPolygon.Points.Add(FPL5);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(FPL5);

                    spMain = new TSG.Point((FPL5.X + p3.X) / 2, FPL5.Y - 200.0, FPL5.Z);
                    epMain = new TSG.Point((FPL5.X + p3.X) / 2, FPL0.Y, FPL0.Z);
                    break;

                case "bot" when dropPosition == "left":
                    dropDepth = Math.Abs(FPL2.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL0.X, FPL0.Y + beamWidth, FPL0.Z);
                    p2 = new TSG.Point(FPL5.X, FPL5.Y + beamWidth, FPL5.Z);
                    RebarPolygonDrop.Points.Add(FPL0);
                    RebarPolygonDrop.Points.Add(FPL5);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(FPL0);

                    spDrop = new TSG.Point(FPL0.X, FPL0.Y + beamWidth / 2, FPL0.Z);
                    epDrop = new TSG.Point(FPL1.X, FPL1.Y + beamWidth / 2, FPL1.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL3.X, FPL3.Y + beamWidth, FPL3.Z);
                    p4 = new TSG.Point(FPL4.X, FPL4.Y + beamWidth, FPL4.Z);
                    RebarPolygon.Points.Add(FPL3);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(FPL3);

                    spMain = new TSG.Point(FPL3.X, FPL3.Y + beamWidth / 2, FPL3.Z);
                    epMain = new TSG.Point(FPL2.X, FPL2.Y + beamWidth / 2, FPL3.Z);

                    break;

                case "bot" when dropPosition == "right":
                    dropDepth = Math.Abs(FPL5.Z - FPL0.Z);
                    p1 = new TSG.Point(FPL1.X, FPL1.Y + beamWidth, FPL1.Z);
                    p2 = new TSG.Point(FPL2.X, FPL2.Y + beamWidth, FPL2.Z);
                    RebarPolygon.Points.Add(FPL1);
                    RebarPolygon.Points.Add(FPL2);
                    RebarPolygon.Points.Add(p2);
                    RebarPolygon.Points.Add(p1);
                    RebarPolygon.Points.Add(FPL1);

                    spDrop = new TSG.Point(FPL0.X, FPL0.Y + beamWidth / 2, FPL0.Z);
                    epDrop = new TSG.Point(FPL1.X, FPL1.Y + beamWidth / 2, FPL1.Z);
                    //--------------------

                    p3 = new TSG.Point(FPL4.X, FPL4.Y + beamWidth, FPL4.Z);
                    p4 = new TSG.Point(FPL3.X, FPL3.Y + beamWidth, FPL3.Z);
                    RebarPolygonDrop.Points.Add(FPL4);
                    RebarPolygonDrop.Points.Add(FPL3);
                    RebarPolygonDrop.Points.Add(p3);
                    RebarPolygonDrop.Points.Add(p4);
                    RebarPolygonDrop.Points.Add(FPL4);

                    spMain = new TSG.Point(FPL4.X, FPL4.Y + beamWidth / 2, FPL4.Z);
                    epMain = new TSG.Point(FPL5.X, FPL5.Y + beamWidth / 2, FPL5.Z);
                    break;

                case "top" when dropPosition == "left":
                    dropDepth = Math.Abs(FPL2.Z - FPL1.Z);

                    p1 = new TSG.Point(FPL2.X, FPL2.Y - beamWidth, FPL2.Z);
                    p2 = new TSG.Point(FPL3.X, FPL3.Y - beamWidth, FPL3.Z);
                    RebarPolygonDrop.Points.Add(FPL2);
                    RebarPolygonDrop.Points.Add(FPL3);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(FPL2);

                    spDrop = new TSG.Point(FPL2.X, FPL2.Y - beamWidth / 2, FPL2.Z);
                    epDrop = new TSG.Point(FPL1.X, FPL1.Y - beamWidth / 2, FPL1.Z);
                    //--------------------

                    p3 = new TSG.Point(FPL5.X, FPL5.Y - beamWidth, FPL5.Z);
                    p4 = new TSG.Point(FPL4.X, FPL4.Y - beamWidth, FPL4.Z);
                    RebarPolygon.Points.Add(FPL5);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(FPL5);

                    spMain = new TSG.Point(FPL5.X, FPL5.Y - beamWidth / 2, FPL5.Z);
                    epMain = new TSG.Point(FPL0.X, FPL0.Y - beamWidth / 2, FPL0.Z);
                    break;

                case "top" when dropPosition == "right":
                    dropDepth = Math.Abs(FPL0.Z - FPL1.Z);
                    p1 = new TSG.Point(FPL0.X, FPL0.Y - beamWidth, FPL0.Z);
                    p2 = new TSG.Point(FPL5.X, FPL5.Y - beamWidth, FPL5.Z);
                    RebarPolygonDrop.Points.Add(FPL0);
                    RebarPolygonDrop.Points.Add(FPL5);
                    RebarPolygonDrop.Points.Add(p2);
                    RebarPolygonDrop.Points.Add(p1);
                    RebarPolygonDrop.Points.Add(FPL0);

                    spDrop = new TSG.Point(FPL0.X, FPL0.Y - beamWidth / 2, FPL0.Z);
                    epDrop = new TSG.Point(FPL1.X, FPL1.Y - beamWidth / 2, FPL1.Z);
                    //--------------------
                    p3 = new TSG.Point(FPL3.X, FPL3.Y - beamWidth, FPL3.Z);
                    p4 = new TSG.Point(FPL4.X, FPL4.Y - beamWidth, FPL4.Z);
                    RebarPolygon.Points.Add(FPL3);
                    RebarPolygon.Points.Add(FPL4);
                    RebarPolygon.Points.Add(p4);
                    RebarPolygon.Points.Add(p3);
                    RebarPolygon.Points.Add(FPL3);

                    spMain = new TSG.Point(FPL2.X, FPL2.Y - beamWidth / 2, FPL2.Z);
                    epMain = new TSG.Point(FPL3.X, FPL3.Y - beamWidth / 2, FPL3.Z);
                    break;
            }

            InsertRebarInfo(spDrop, epDrop, RebarPolygonDrop, beam, radius, rebarName, beamOriented, rebarSpacing, dropPosition);
            InsertRebarInfo(spMain, epMain, RebarPolygon, beam, radius, rebarName, beamOriented, rebarSpacing, dropPosition);
        }

#warning Links bar info

        private static void BeamStirrupBarInfo(Beam beam, string beamOriented, double radiusDouble, RebarGroup rebar, ArrayList rebarSpacing, string dropPosition)
        {
            rebar.RadiusValues.Add(2 * radiusDouble);
            RebarHookData customHook = new RebarHookData() { Shape = RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, Angle = 90.0, Radius = 16.0, Length = 81.0 };
            //rebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK;
            //rebar.StartHook.Angle = 90.0;
            //rebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK;'
            rebar.StartHook = customHook;
            rebar.EndHook = customHook;

            rebar.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            rebar.Spacings = rebarSpacing;
            rebar.StartPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
            rebar.EndPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_COVER_THICKNESS;
            //Hard code for radiusDouble = 8
            rebar.StartPointOffsetValue = 30.0;
            rebar.EndPointOffsetValue = 30.0;

            //PlanOffsets
#warning Can chack lai cong thuc cho link bars
            ArrayList onPlanOffsets = null;
            /*
            if (beamOriented == "hor")
            {
                onPlanOffsets = new ArrayList
            {
                (200 - 38 - radiusDouble) * -1,
                0.0,
                (200 - 38 - radiusDouble) * -1
            };
            }
            else if (beamOriented == "ver")
            {
                onPlanOffsets = new ArrayList
            {
                (38 + radiusDouble),
                0.0,
                (38 + radiusDouble)
            };
            };
            if (onPlanOffsets == null)
            {
                MessageBox.Show("false");
            }
            */
            onPlanOffsets = new ArrayList { 30.0 };
            rebar.OnPlaneOffsets = onPlanOffsets;
            if (dropPosition == null)
            {
                if (beamOriented == "hor")
                {
                    rebar.StartFromPlaneOffset = 30.0;
                    rebar.EndFromPlaneOffset = 30.0;
                }
                else if (beamOriented == "ver")
                {
                    rebar.StartFromPlaneOffset = 200.0;
                    rebar.EndFromPlaneOffset = 200.0;
                };
            }
            else
            {
                rebar.StartFromPlaneOffset = 30.0;
                rebar.EndFromPlaneOffset = 30.0;
            }

            rebar.ExcludeType = RebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
            rebar.Father = beam;
            rebar.Insert();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Model model = new Model();
            Picker Picker = new Picker();
            try
            {
                PickInput Input = Picker.PickFace("");
                IEnumerator MyEnum = Input.GetEnumerator();
                while (MyEnum.MoveNext())
                {
                    InputItem Item = MyEnum.Current as InputItem;
                    if (Item.GetInputType() == InputItem.InputTypeEnum.INPUT_1_OBJECT)
                    {
                        ModelObject M = Item.GetData() as ModelObject;
                        MessageBox.Show(M.Identifier.ToString());
                    }
                    if (Item.GetInputType() == InputItem.InputTypeEnum.INPUT_POLYGON)
                    {
                        ArrayList Points = Item.GetData() as ArrayList;
                        MessageBox.Show((Points[0] as TSG.Point).ToString());
                        ControlPoint controlPoint = new ControlPoint(Points[0] as TSG.Point);
                        controlPoint.Insert();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            model.CommitChanges();
        }

        private void rmPoints_btn_Click(object sender, EventArgs e)
        {
            Model model = new Model();
            TSM.ModelObjectSelector selector = model.GetModelObjectSelector();
            var controlPoints = selector.GetAllObjectsWithType(ModelObject.ModelObjectEnum.CONTROL_POINT);
            while (controlPoints.MoveNext())
            {
                controlPoints.Current.Delete();
            }

            var allRebar = selector.GetAllObjectsWithType(ModelObject.ModelObjectEnum.REBARGROUP);
            while (allRebar.MoveNext())
            {
                allRebar.Current.Delete();
            }
            model.CommitChanges();
        }

        private void btn_Quit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}