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

namespace RebarCreate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Model myModel = new Model();
                ModelObjectEnumerator myEnum = new TSMUI.ModelObjectSelector().GetSelectedObjects();
                while (myEnum.MoveNext())
                {
                    Beam myPart = myEnum.Current as Beam;
                    if (myPart != null)
                    {
                        WorkPlaneHandler workPlane =
                            myModel.GetWorkPlaneHandler();
                        TransformationPlane currentPlane =
                            workPlane.GetCurrentTransformationPlane();
                        TransformationPlane localPlane =
                            new TransformationPlane(myPart.GetCoordinateSystem());
                        workPlane.SetCurrentTransformationPlane(localPlane);
                        Solid solid = myPart.GetSolid() as Solid;
                        TSM.Component component1 = new TSM.Component();
                        component1.Number = 1000001;
                        component1.LoadAttributesFromFile("standard");
                        component1.SetAttribute("bar1_no", 4);
                        component1.SetAttribute("cc_side", 45);
                        component1.SetAttribute("cc_bottom", 45);

                        TSG.Point p1 = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MaximumPoint.Z);
                        TSG.Point p2 = new TSG.Point(solid.MaximumPoint.X, solid.MinimumPoint.Y, solid.MaximumPoint.Z);
                        TSG.Point p3 = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z);

                        ComponentInput input1 = new ComponentInput();
                        input1.AddInputObject(myPart);
                        input1.AddTwoInputPositions(p1, p2);
                        input1.AddOneInputPosition(p3);

                        component1.SetComponentInput(input1);

                        component1.Insert();
                        if (!component1.Insert())
                        {
                            Console.WriteLine("Component Insert failed");
                        }
                    }
                }
                myModel.CommitChanges();
            }
            catch (Exception)
            {
                throw;
            }
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
                ModelObjectEnumerator myEnum = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                while (myEnum.MoveNext())
                {
                    Beam beam = myEnum.Current as Beam;
                    if (beam != null)
                    {
                        WorkPlaneHandler workPlane =
                            myModel.GetWorkPlaneHandler();
                        TransformationPlane currentPlane =
                            workPlane.GetCurrentTransformationPlane();
                        TransformationPlane localPlane =
                            new TransformationPlane(beam.GetCoordinateSystem());

                        workPlane.SetCurrentTransformationPlane(localPlane);
                        RebarGroup rebar = new RebarGroup();
                        Polygon RebarPolygon1 = new Polygon();
                        //WorkPlane
                        Solid solid = beam.GetSolid();

                        double MinX = solid.MinimumPoint.X;
                        double MinY = solid.MinimumPoint.Y;
                        double MinZ = solid.MinimumPoint.Z;
                        double MaxX = solid.MaximumPoint.X;
                        double MaxY = solid.MaximumPoint.Y;
                        double MaxZ = solid.MaximumPoint.Z;

                        TSG.Point p1 = new TSG.Point(MinX, MinY + 46, MinZ);
                        TSG.Point p2 = new TSG.Point(MinX, MinY + 46, MaxZ);
                        TSG.Point p3 = new TSG.Point(MaxX, MinY + 46, MaxZ);
                        TSG.Point p4 = new TSG.Point(MaxX, MinY + 46, MinZ);

                        ArrayList pointList = new ArrayList();

                        //TSG.Point sp = beam.GetCenterLine(true)[0] as TSG.Point;
                        //TSG.Point ep = beam.GetCenterLine(true)[1] as TSG.Point;

                        TSG.Point sp = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MaxZ);
                        TSG.Point ep = new TSG.Point((MinX + MaxX) / 2, MinY + 46, MinZ);

                        RebarPolygon1.Points.Add(p1);
                        RebarPolygon1.Points.Add(p2);
                        RebarPolygon1.Points.Add(p3);
                        RebarPolygon1.Points.Add(p4);

                        rebar.Polygons.Add(RebarPolygon1);

                        rebar.StartPoint = p3;
                        rebar.EndPoint = p4;
                        rebar.Name = "TOP BAR";
                        rebar.Grade = "H";
                        rebar.Size = "16";
                        rebar.RadiusValues.Add(32.0);
                        //rebar.Class = 2;

                        //rebar.StartHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;
                        //rebar.EndHook.Shape = RebarHookData.RebarHookShapeEnum.NO_HOOK;

                        //rebar.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_EXACT_NUMBER;

                        //ArrayList numOfRebar = new ArrayList() { 2 };
                        //rebar.Spacings = numOfRebar;
                        //rebar.StartPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
                        //rebar.StartPointOffsetValue = 390;
                        //rebar.EndPointOffsetType = Reinforcement.RebarOffsetTypeEnum.OFFSET_TYPE_LEG_LENGTH;
                        //rebar.EndPointOffsetValue = 390;

                        //ArrayList onPlanOffsets = new ArrayList() { 46, 0, 46 };
                        //rebar.OnPlaneOffsets = onPlanOffsets;

                        //rebar.ExcludeType = RebarGroup.ExcludeTypeEnum.EXCLUDE_TYPE_NONE;
                        //rebar.Father = beam;

                        //rebar.StartFromPlaneOffset = 36;
                        //rebar.EndFromPlaneOffset = 46;

                        rebar.Insert();

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

        private void button3_Click(object sender, EventArgs e)
        {
        }
    }
}