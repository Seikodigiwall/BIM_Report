__title__ = 'Create New Data'
__doc__ = "Create New Data to database."\


from Autodesk.Revit.DB import *
from Autodesk.Revit import DB
from rpw.ui.forms import SelectFromList


uidoc = __revit__.ActiveUIDocument
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application

# create an empty list / variables
Param1 = [] # parameter names
value1 = [] # parameter values for each name
Param2 = []
value2 = []
Param3 = []
value3 = []
Param4 = []
value4 = []
Param5 = []
value5 = []
Param6 = []
value6 = []
Param7 = []
value7 = []
Param8 = []
value8 = []

# method to get the selected element ID
def GetSelectedElementIds(doc):
    return [doc.GetElement(id)
            for id in __revit__.ActiveUIDocument.Selection.GetElementIds()]
selection = GetSelectedElementIds(doc) # calls the method and stores output in selection

#database codes
import clr
import System
clr.AddReference("System.Data")
from System.Data import DataSet
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
connection = OdbcConnection(connectString)
connection.Open()

table = SelectFromList('Select Table', ['bim_r515'])

if 0 == selection.Count:
    print ("You haven't selected any element/s.")
else:
    for el in selection:
        selectedElementId = el.Id # from '12345' to 12345
                              # assign formatted output into selectedElementId
        selectedElement = Document.GetElement(doc, selectedElementId) # GetElement method gets the element referenced by the input element ID

        for parameter1 in selectedElement.GetParameters("Mark"):
            Param1.append(parameter1.Definition.Name)
            value1.append(parameter1.AsString())
            dbParam1 = parameter1.Definition.Name
            dbVal1 = parameter1.AsString()

        for parameter2 in selectedElement.GetParameters("Description"):
            Param2.append(parameter2.Definition.Name)
            value2.append(parameter2.AsString())
            dbParam2 = parameter2.Definition.Name
            dbVal2 = parameter2.AsString()

        for parameter3 in selectedElement.GetParameters("Tag_Number"):
            Param3.append(parameter3.Definition.Name)
            value3.append(parameter3.AsString())
            dbParam3 = parameter3.Definition.Name
            dbVal3 = parameter3.AsString()

        for parameter4 in selectedElement.GetParameters("Type_Assembly"):
            Param4.append(parameter4.Definition.Name)
            value4.append(parameter4.AsString())
            dbParam4 = parameter4.Definition.Name
            dbVal4 = parameter4.AsString()

        for parameter5 in selectedElement.GetParameters("FO_Number"):
            Param5.append(parameter5.Definition.Name)
            value5.append(parameter5.AsString())
            dbParam5 = parameter5.Definition.Name
            dbVal5 = parameter5.AsString()

        for parameter6 in selectedElement.GetParameters("M_Level"):
            Param6.append(parameter6.Definition.Name)
            value6.append(parameter6.AsString())
            dbParam6 = parameter6.Definition.Name
            dbVal6 = parameter6.AsString()

        for parameter7 in selectedElement.GetParameters("Quantity"):
            Param7.append(parameter7.Definition.Name)
            value7.append(parameter7.AsString())
            dbParam7 = parameter7.Definition.Name
            dbVal7 = parameter7.AsString()

        for parameter8 in selectedElement.GetParameters("Status"):
            Param8.append(parameter8.Definition.Name)
            value8.append(parameter8.AsString())
            dbParam8 = parameter8.Definition.Name
            dbVal8 = parameter8.AsString()



        query = "INSERT INTO %s (ELEMENT_ID,Mark,Description,Tag_Number,Type_Assembly,FO_Number,M_Level,Quantity,Status) SELECT '%s','%s','%s','%s','%s','%s','%s','%s','%s'" % (table,selectedElementId,dbVal1,dbVal2,dbVal3,dbVal4,dbVal5,dbVal6,dbVal7,dbVal8)
        print(query)
        command = OdbcCommand(query, connection)
        command.ExecuteNonQuery()

    print("Parameter: {}".format(Param1))
    print("Value:     {}".format(value1))
    print("Parameter: {}".format(Param2))
    print("Value:     {}".format(value2))
    print("Parameter: {}".format(Param3))
    print("Value:     {}".format(value3))
    print("Parameter: {}".format(Param4))
    print("Value:     {}".format(value4))
    print("Parameter: {}".format(Param5))
    print("Value:     {}".format(value5))
    print("Parameter: {}".format(Param6))
    print("Value:     {}".format(value6))
    print("Parameter: {}".format(Param7))
    print("Value:     {}".format(value7))
    print("Parameter: {}".format(Param8))
    print("Value:     {}".format(value8))

#connection.Close()
