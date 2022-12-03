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

using TSM = Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

using System.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Tekla.Structures.ModelInternal;

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
                        else if (gapX >= 0 && gapX <= 1)
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
                        //MessageBox.Show(beamPosition);

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

                        #endregion Solid

                        CreateTopRebar(beam, MinX, MinZ, MaxX, MinY, MaxY, MaxZ, "13", beamOriented, beamPosition);
                        CreateBotRebar(beam, MinX, MaxY, MinY, MinZ, MaxX, MaxZ, "16", beamOriented, beamPosition);
#warning Reset working plan
                        workPlane.SetCurrentTransformationPlane(currentPlane);
                        myModel.CommitChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
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

        private void CreateBotRebar(Beam beam, double MinX, double MaxY, double MinY, double MinZ, double MaxX, double MaxZ, string radius, string beamOriented, string beamPosition)
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

            //TSG.Point sp = beam.GetCenterLine(true)[0] as TSG.Point;
            //TSG.Point ep = beam.GetCenterLine(true)[1] as TSG.Point;

            InsertBeamInfo(spBot, epBot, RebarPolygonBot, beam, radius, "BOT BAR", beamOriented);

            #endregion Bot rebar
        }

        private void CreateTopRebar(Beam beam, double MinX, double MinZ, double MaxX, double MinY, double MaxY, double MaxZ, string radius, string beamOriented, string beamPosition)
        {
            #region Top rebar

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

            InsertBeamInfo(spTop, epTop, RebarPolygonTop, beam, radius, "TOP BAR", beamOriented);

            #endregion Top rebar
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

        public void InsertBeamInfo(TSG.Point sp, TSG.Point ep, Polygon RebarPolygonBot, Beam beam, string radius, string rebarName, string beamOriented)
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

            rebar.Polygons.Add(RebarPolygonBot);
            rebar.RadiusValues.Add(2 * radiusDouble);
            rebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
            rebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;

            rebar.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_NUMBER;

            ArrayList numOfRebar = new ArrayList() { 2.0 };
            rebar.Spacings = numOfRebar;
            rebar.StartPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            rebar.EndPointOffsetType =
                Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
            if (radius == "16")
            {
                rebar.StartPointOffsetValue = 390;
                rebar.EndPointOffsetValue = 390;
            }
            else if (radius == "13")
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

            //if (rebar.Insert())
            //{
            //    MessageBox.Show("Success!");
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Model model = new Model();
            Beam MyBeam = new Beam();
            MyBeam.DeformingData.Angle = 0.0;
            MyBeam.DeformingData.Angle2 = 45.00;
            MyBeam.DeformingData.Cambering = 10.0;
            MyBeam.DeformingData.Shortening = 20.0;
            MyBeam.Insert();
            model.CommitChanges();
        }
    }
}