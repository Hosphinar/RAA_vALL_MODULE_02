#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;

#endregion

namespace RAA_vALL_MODULE_02
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge : IExternalCommand
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
            //The "using" statement will automatically dispose of the transaction at the closing bracket.
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit elements");

                //Use FilteredElementCollectors to get al the requested Revit elements.
                //Get wall types
                FilteredElementCollector wallCollector = new FilteredElementCollector(doc);
                wallCollector.OfClass(typeof(WallType));

                WallType wallTypeStrfrnt = null;
                WallType wallTypeGeneric = null;
                foreach (WallType curWallType in wallCollector)
                {
                    switch (curWallType.Name)
                    {
                        case "Storefront":
                            wallTypeStrfrnt = curWallType;
                            break;

                        case "Generic - 8\"":
                            wallTypeGeneric = curWallType;
                            break;

                        default:
                            break;
                    }
                }

                //Get MEP system types
                FilteredElementCollector MEPSystemCollector = new FilteredElementCollector(doc);
                MEPSystemCollector.OfClass(typeof(MEPSystemType));

                MEPSystemType ductSystemType = null;
                MEPSystemType pipeSystemType = null;
                foreach (MEPSystemType curSystemType in MEPSystemCollector)
                {
                    switch (curSystemType.Name)
                    {
                        case "Supply Air":
                            ductSystemType = curSystemType;
                            break;

                        case "Hydronic Supply":
                            pipeSystemType = curSystemType;
                            break;

                        default:
                            break;
                    }
                }

                //Get default MEP element types for duct and pipe.
                FilteredElementCollector ductCollector = new FilteredElementCollector(doc);
                ductCollector.OfClass(typeof(DuctType));
                //Why can't I assign this to the first element prior to the create method?
                //DuctType ductType = ductCollector.FirstElement();
                
                FilteredElementCollector pipeCollector = new FilteredElementCollector(doc);
                pipeCollector.OfClass(typeof(PipeType));
                //PipeType pipeType = null;

                //Initialize element counters.
                int numWallsStrfrnt = 0;
                int numWallsGeneric = 0;
                int numDuctsDefault = 0;
                int numPipesDefault = 0;
                int numElementsNotAdded = 0;

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
                            Wall.Create(doc, wallCurveStrfrnt, wallTypeStrfrnt.Id, newLevel.Id, 20, 0, false, false);
                            numWallsStrfrnt++;
                            break;

                        case "A-WALL":
                            Curve wallCurveGeneric = curve.GeometryCurve;
                            Wall.Create(doc, wallCurveGeneric, wallTypeGeneric.Id, newLevel.Id, 20, 0, false, false);
                            numWallsGeneric++;
                            break;

                        case "M-DUCT":
                            Curve ductCurve = curve.GeometryCurve;
                            Duct.Create(doc, ductSystemType.Id, ductCollector.FirstElementId(), newLevel.Id, ductCurve.GetEndPoint(0), ductCurve.GetEndPoint(1));
                            numDuctsDefault++;
                            break;

                        case "P-PIPE":
                            Curve pipeCurve = curve.GeometryCurve;
                            Pipe.Create(doc, pipeSystemType.Id, pipeCollector.FirstElementId(), newLevel.Id, pipeCurve.GetEndPoint(0), pipeCurve.GetEndPoint(1));
                            numPipesDefault++;
                            break;
                        
                        default:
                            numElementsNotAdded++;
                            break;
                    }
                }

                //Alert user
                TaskDialog.Show("Notice", "Selected " + pickList.Count.ToString() + " elements, but only " + selectedModelLines + " valid lines.");
                TaskDialog.Show("Notice", "Created the following elements: \n" +  numWallsStrfrnt + " - Storefront Walls\n" + numWallsGeneric + " - Generic 8\" Walls\n" + numDuctsDefault + " - Default Ducts\n" + 
                    numPipesDefault + " - Default Pipes\n" + numElementsNotAdded + " - Selected elements were not added");

                t.Commit();
            }



            /* 



                   WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                   WallType wallType2 = GetWallTypeByName(doc, "Exterior - Brick on CMU");





                   WallType myWallType = GetWallTypeByName(doc, "Basic Wall");
                   Wall.Create(doc, curCurve2, myWallType.Id, newLevel.Id, 20, 0, false, false);














               /*
               //Use the custom Methods
               string testString = MyFirstMethod();
               MySecondMethod();
               string testString2 = MyThirdMethod("Hello World!");
               */

            //t.Commit();



            return Result.Succeeded;



        }

        /* Random Methods
        //Make 3 different method types.
        internal string MyFirstMethod()
        {
            return "This is my first method!";
        }
        internal void MySecondMethod()
        {
            Debug.Print("This is my second method!");
        }
        internal string MyThirdMethod(string input)
        {
            return "This is my third method: " + input;
        }
        */








        //We will only use this method for the Bonus portion.
        /*
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
        */
        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
