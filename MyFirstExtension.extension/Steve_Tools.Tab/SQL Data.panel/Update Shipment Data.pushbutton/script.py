__title__ = 'Update Container No'
__doc__ = "Updates ALL changes from database."\

from Autodesk.Revit import DB
from rpw.ui.forms import SelectFromList

import clr
clr.AddReference("RevitAPI")
from Autodesk.Revit.DB import *
import Autodesk

clr.AddReference("RevitServices")
import RevitServices
from RevitServices.Persistence import DocumentManager

from System.Collections.Generic import *

uidoc = __revit__.ActiveUIDocument
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application

# connecting to database
import clr
import System
clr.AddReference("System.Data")
from System.Data import *
from System.Data.Odbc import OdbcConnection, OdbcDataAdapter, OdbcCommand, OdbcParameter, OdbcType 
connectString = (
"DRIVER={MySQL ODBC 8.0 ANSI Driver};"
"SERVER=192.168.100.108;"
"PORT=3306;"
"DATABASE=skidd_bim;"
"USER=seikowall;"
"PASSWORD=IDDQR;"
"OPTION=3;"
)

# Set up connection
connection = OdbcConnection(connectString)
connection.Open()

table = SelectFromList('Select table to retrieve information from.', ['bim_r515'])

# Get ALL the element ids that is in the database 
query = "SELECT ELEMENT_ID FROM %s" %(table) # query statement retrieves all the element ids in the database 
command = OdbcCommand(query, connection)
reader = command.ExecuteReader() 

#Collects all window and wall elements
builtInElements = List[BuiltInCategory]()
builtInElements.Add(BuiltInCategory.OST_Windows)
builtInElements.Add(BuiltInCategory.OST_Walls)
builtInElements.Add(BuiltInCategory.OST_CurtainWallPanels)
builtInElements.Add(BuiltInCategory.OST_Curtain_Systems)
builtInElements.Add(BuiltInCategory.OST_StructuralFraming )

filter1 = ElementMulticategoryFilter(builtInElements)
cl = FilteredElementCollector(doc).WherePasses(filter1).WhereElementIsNotElementType()

transaction = Transaction(doc, "Update information from DB")

# Reads for Element ID
while reader.Read():
    resultForId = reader[0]
    #print(resultForId) #WORKING
    queryForPara = "SELECT * FROM %s WHERE %s.ELEMENT_ID = '%s'" %(table,table,resultForId)
    #print(queryForPara) #WORKING
    commandForPara = OdbcCommand(queryForPara, connection)
    readerForPara = commandForPara.ExecuteReader()

# Reads for Parameter Value
    while readerForPara.Read():
        resultForPara = readerForPara[10]
        resultForPara2 = readerForPara[11]
        resultForPara3 = readerForPara[12]
        resultForPara4 = readerForPara[13]
       
   
        #print(resultForPara) #WORKING
        for wId in cl.ToElementIds(): #WORKING
            #print winId.ToString()
            #print resultForId
            if wId.ToString() == resultForId: 
                wElement = Document.GetElement(doc, wId)
                wPara = wElement.GetParameters("Packing_List")
                wPara2 = wElement.GetParameters("Crate_No") 
                wPara3 = wElement.GetParameters("Container") 
                wPara4 = wElement.GetParameters("ETD") 
               

                transaction.Start()
                try:
                    updateStatus = wPara[0].Set(resultForPara)
                    updateStatus2 = wPara2[0].Set(resultForPara2)
                    updateStatus3 = wPara3[0].Set(resultForPara3)
                    updateStatus4 = wPara4[0].Set(resultForPara4)
                    
               
                    if updateStatus:   
                        print("Container Updated")
                
                        transaction.Commit()
                except:
                    print("ERROR.")
                    transaction.RollBack()


