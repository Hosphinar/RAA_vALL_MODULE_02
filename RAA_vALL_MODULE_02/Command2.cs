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
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            /*
            // Pick Elements by Rectangle and filter into a list.
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select elements");

            TaskDialog.Show("Test", "I selected " + pickList.Count.ToString() + " elements.");

            //Filter selected elements for curves.
            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in pickList) 
            { 
                if (elem is CurveElement)
                {
                    //This is casting the element from a generic type to a CurveElement type.
                    allCurves.Add(elem as CurveElement);
                }
            }

            //Filter selected elements for model curves.
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement)
                {
                    CurveElement curveElem = elem as CurveElement;
                    //An alternate cast method is below:
                    //CurveElement curveElem = (CurveElement) elem;

                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            //Curve data
            foreach (CurveElement currentCurve in modelCurves)
            {
                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);

                GraphicsStyle curStyle = currentCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            //Create Transaction with Using statement
            //Using statement will automatically dispose of the transaction at the closing bracket.
            using (Transaction t = new Transaction(doc)) 
            {
                t.Start("Create Revit elements");

                //Create wall
                Level newLevel = Level.Create(doc, 20);
                Curve curCurve1 = modelCurves[0].GeometryCurve;

                //Edited out from last step of workshop!
                //Wall.Create(doc, curCurve1, newLevel.Id, false);

                FilteredElementCollector wallTypes = new FilteredElementCollector(doc);
                wallTypes.OfClass(typeof(WallType));

                Curve curCurve2 = modelCurves[1].GeometryCurve;
                WallType myWallType = GetWallTypeByName(doc, "Basic Wall");
                Wall.Create(doc, curCurve2, myWallType.Id, newLevel.Id, 20, 0, false, false);
                //Below is without creating a custom WallType method.
                //Wall.Create(doc, curCurve2, wallTypes.FirstElementId(), newLevel.Id, 20, 0, false, false); 

                //Get MEP system types
                FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                systemCollector.OfClass(typeof(MEPSystemType));

                //Get duct system types from MEP system types.
                MEPSystemType ductSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Supply Air")
                    {
                        ductSystemType = curType;
                        break;
                    }
                }

                //Get duct type
                FilteredElementCollector collector1 = new FilteredElementCollector(doc);
                collector1.OfClass(typeof(DuctType));

                //Create duct
                Curve curCurve3 = modelCurves[2].GeometryCurve;
                Duct newDuct = Duct.Create(doc, ductSystemType.Id, collector1.FirstElementId(), newLevel.Id, curCurve3.GetEndPoint(0), curCurve3.GetEndPoint(1));

                //Get pipe system types from MEP system types.
                MEPSystemType pipeSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Domestic Hot Water")
                    {
                        pipeSystemType = curType;
                        break;
                    }
                }

                //Get pipe type
                FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                collector2.OfClass(typeof(PipeType));

                //Create pipe
                Curve curCurve4 = modelCurves[3].GeometryCurve;
                Pipe newPipe = Pipe.Create(doc, pipeSystemType.Id, collector2.FirstElementId(), newLevel.Id, curCurve4.GetEndPoint(0), curCurve4.GetEndPoint(1));

                //Use the custom Methods
                string testString = MyFirstMethod();
                MySecondMethod();
                string testString2 = MyThirdMethod("Hello World!");

                //Switch statement
                int numberValue = 5;
                string numAsString = "";

                switch (numberValue)
                {
                    case 1:
                        numAsString = "One";
                        break;

                    case 2:
                        numAsString = "Two";
                        break;

                    case 3:
                        numAsString = "Three";
                        break;

                    case 4:
                        numAsString = "Four";
                        break;

                    case 5:
                        numAsString = "Five";
                        break;

                    default:
                        numAsString = "Zero";
                        break;
                }

                //Advanced Switch statement
                Curve curve5 = modelCurves[1].GeometryCurve;
                GraphicsStyle curve5GS = modelCurves[1].LineStyle as GraphicsStyle;

                WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                WallType wallType2 = GetWallTypeByName(doc, "Exterior - Brick on CMU");

                switch (curve5GS.Name)
                {
                    case "<Thin Lines":
                        Wall.Create(doc, curve5, wallType1.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    case "<Wide Lines":
                        Wall.Create(doc, curve5, wallType2.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    default:
                        Wall.Create(doc, curve5, newLevel.Id, false);
                        break;
                }

                t.Commit();
            }
            */
            return Result.Succeeded;
        }

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
        
        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
