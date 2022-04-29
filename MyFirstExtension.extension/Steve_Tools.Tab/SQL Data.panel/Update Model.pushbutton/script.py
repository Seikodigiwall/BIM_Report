__title__ = 'Update Model'
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
        resultForPara = readerForPara[2]
        resultForPara2 = readerForPara[3]
        resultForPara3 = readerForPara[4]
        resultForPara4 = readerForPara[5]
        resultForPara5 = readerForPara[6]
        resultForPara6 = readerForPara[7]
        resultForPara7 = readerForPara[8]
        resultForPara8 = readerForPara[9]
        resultForPara9 = readerForPara[10]
        resultForPara10 = readerForPara[11]
        resultForPara11 = readerForPara[12]
        resultForPara12 = readerForPara[13]
   
        #print(resultForPara) #WORKING
        for wId in cl.ToElementIds(): #WORKING
            #print winId.ToString()
            #print resultForId
            if wId.ToString() == resultForId: 
                wElement = Document.GetElement(doc, wId)
                wPara = wElement.GetParameters("Mark")
                wPara2 = wElement.GetParameters("Description") 
                wPara3 = wElement.GetParameters("Tag_Number")  
                wPara4 = wElement.GetParameters("Type_Assembly") 
                wPara5 = wElement.GetParameters("FO_Number") 
                wPara6 = wElement.GetParameters("M_Level") 
                wPara7 = wElement.GetParameters("Quantity") 
                wPara8 = wElement.GetParameters("Status") 
                wPara9 = wElement.GetParameters("Packing_List") 
                wPara10 = wElement.GetParameters("Crate_No") 
                wPara11 = wElement.GetParameters("Container") 
                wPara12 = wElement.GetParameters("ETD") 

                transaction.Start()
                try:
                    updateStatus = wPara[0].Set(resultForPara)
                    updateStatus2 = wPara2[0].Set(resultForPara2)
                    updateStatus3 = wPara3[0].Set(resultForPara3)
                    updateStatus4 = wPara4[0].Set(resultForPara4)
                    updateStatus5 = wPara5[0].Set(resultForPara5)
                    updateStatus6 = wPara6[0].Set(resultForPara6)
                    updateStatus7 = wPara7[0].Set(resultForPara7)
                    updateStatus8 = wPara8[0].Set(resultForPara8)
                    updateStatus9 = wPara9[0].Set(resultForPara9)
                    updateStatus10 = wPara10[0].Set(resultForPara10)
                    updateStatus11 = wPara11[0].Set(resultForPara11)
                    updateStatus12 = wPara12[0].Set(resultForPara12)
               
                    if updateStatus:   
                        print("Data Updated")
                
                        transaction.Commit()
                except:
                    print("ERROR.")
                    transaction.RollBack()

