#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Controls;

#endregion

namespace RAA_vALL_MODULE_02
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge_Bonus : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Pick Elements by Rectangle and filter into a list.
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select model line elements to convert");

            //Filter selected elements for model curves.
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement)
                {
                    CurveElement curveElem = elem as CurveElement;

                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            //Create a counter for the lines that are actual model curves.
            int selectedModelLines = 0; 
            int numBoundLines = 0;

            //Get curve data and filter the selection.
            //Is there a way to condense all these foreach loops?
            foreach (CurveElement currentCurve in modelCurves)
            {
                Curve curve = currentCurve.GeometryCurve;
                //Does this need to be here?
                XYZ startPoint = null;
                XYZ endPoint = null;

                //Correct the selection for Circles.
                if (curve.IsBound == true)
                {
                    startPoint = curve.GetEndPoint(0);
                    endPoint = curve.GetEndPoint(1);
                    numBoundLines++;
                }
                selectedModelLines++;
            }

            //Create Transaction with Using statement
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit elements");

                //Initialize element counters.
                int numWallsStrfrnt = 0;
                int numWallsGeneric = 0;
                int numDuctsDefault = 0;
                int numPipesDefault = 0;
                int numElementsNotAdded = 0;

                //Make Type names variables to make it easier.
                string wallTypeName1 = "Storefront";
                string wallTypeName2 = "Generic - 8\"";
                string ductSystemName = "Supply Air";
                string ductTypeName = "Default";
                string pipeSystemName = "Other";
                string pipeTypeName = "Default";

                //Create level to place elements.
                Level newLevel = Level.Create(doc, 20);

                //Loop through modelCurves by GraphicsStyle
                foreach (CurveElement curve in modelCurves)
                {
                    GraphicsStyle curveGS = curve.LineStyle as GraphicsStyle;

                    //Switch statement to execute actions based on the Linestyle.
                    switch (curveGS.Name)
                    {
                        case "A-GLAZ":
                            Curve wallCurveStrfrnt = curve.GeometryCurve;
                            WallType wallTypeStrfrnt = GetWallTypeByName(doc, wallTypeName1);
                            CreateWall(doc, wallCurveStrfrnt, wallTypeStrfrnt, newLevel, 10);
                            numWallsStrfrnt++;
                            break;

                        case "A-WALL":
                            Curve wallCurveGeneric = curve.GeometryCurve;
                            WallType wallTypeGeneric = GetWallTypeByName(doc, wallTypeName2);
                            CreateWall(doc, wallCurveGeneric, wallTypeGeneric, newLevel, 10);
                            numWallsGeneric++;
                            break;

                        case "M-DUCT":
                            Curve ductCurve = curve.GeometryCurve;
                            MEPSystemType ductSystemType = GetSystemTypeByName(doc, ductSystemName);
                            DuctType ductType = GetDuctTypeByName(doc, ductTypeName);
                            CreateDuct(doc, ductSystemType, ductType, newLevel, ductCurve);
                            numDuctsDefault++;
                            break;

                        case "P-PIPE":
                            Curve pipeCurve = curve.GeometryCurve;
                            MEPSystemType pipeSystemType = GetSystemTypeByName(doc, pipeSystemName);
                            PipeType pipeType = GetPipeTypeByName(doc, pipeTypeName);
                            CreatePipe(doc, pipeSystemType, pipeType, newLevel, pipeCurve);
                            numPipesDefault++;
                            break;

                        default:
                            numElementsNotAdded++;
                            break;
                    }
                }

                //Alert user
                TaskDialog.Show("Notice", "Selected " + pickList.Count.ToString() + " elements, but only " + selectedModelLines + " valid lines.");
                TaskDialog.Show("Notice", "Created the following elements: \n" + 
                    numWallsStrfrnt + " - " + wallTypeName1 + " Walls\n" + 
                    numWallsGeneric + " - " + wallTypeName2 + " Walls\n" + 
                    numDuctsDefault + " - " + ductSystemName + " " + ductTypeName + " Ducts\n" + 
                    numPipesDefault + " - " + pipeSystemName + " " + pipeTypeName + " Pipes\n" + 
                    numElementsNotAdded + " - Selected elements were not added");

                t.Commit();
            }
            return Result.Succeeded;
        }

        //Get wall type by name method.
        internal WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }

        //Get MEP system type by name method.
        internal MEPSystemType GetSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }

        //Get duct type by name method.
        internal DuctType GetDuctTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }

        //Get pipe type by name method.
        internal PipeType GetPipeTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }

        //Create wall method. (Just for fun)
        internal Wall CreateWall(Document doc, Curve curve, WallType wallType, Level level, double height)
        {
            return Wall.Create(doc, curve, wallType.Id, level.Id, height, 0, false, false);
        }

        //Create duct method.(Just for fun)
        internal Duct CreateDuct(Document doc, MEPSystemType ductSystemType, DuctType ductType, Level level, Curve curve)
        {
            return Duct.Create(doc, ductSystemType.Id, ductType.Id, level.Id, curve.GetEndPoint(0), curve.GetEndPoint(1));
        }

        //Create pipe method.(Just for fun)
        internal Pipe CreatePipe(Document doc, MEPSystemType pipeSystemType, PipeType pipeType, Level level, Curve curve)
        {
            return Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, level.Id, curve.GetEndPoint(0), curve.GetEndPoint(1));
        }

        //Don't recall where this came from exactly.
        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
