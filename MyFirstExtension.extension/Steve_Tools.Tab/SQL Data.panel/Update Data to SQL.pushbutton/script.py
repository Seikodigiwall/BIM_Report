__title__ = 'Update Data'
__doc__ = "Updates model information FROM REVIT TO DATABASE."

from rpw.ui.forms import TextInput
from Autodesk.Revit import DB
from Autodesk.Revit.DB import *
from rpw.ui.forms import SelectFromList


doc = __revit__.ActiveUIDocument.Document

value = []
value2 = []
value3 = []
value4 = []
value5 = []
value6 = []
value7 = []
value8 = []


# This file updates the Project Parameter values from getting a user input

# method to get the selected element ID
def GetSelectedElementIds(doc):
    return [doc.GetElement(id)
            for id in __revit__.ActiveUIDocument.Selection.GetElementIds()]


# calls the method and stores output in selection
selection = GetSelectedElementIds(doc)

# ccreate transaction object
t = Transaction(doc, "Update Parameters")

# database codes
# updates the database with new user input
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

table = SelectFromList('Select table to update changes into.', ['bim_r515'])


for el in selection:
    selectedElementId = el.Id  # from '12345' to 12345
    # assign formatted output into selectedElementId
    # GetElement method gets the element referenced by the input element ID
    selectedElement = Document.GetElement(doc, selectedElementId)

    for parameter in selectedElement.GetParameters("Mark"):
        value.append(parameter.AsString())
        dbVal = parameter.AsString()
        for parameter2 in selectedElement.GetParameters("Description"):
            value2.append(parameter2.AsString())
            dbVal2 = parameter2.AsString()
        for parameter3 in selectedElement.GetParameters("Tag_Number"):
            value3.append(parameter3.AsString())
            dbVal3 = parameter3.AsString()
            for parameter4 in selectedElement.GetParameters("Type_Assembly"):
                value4.append(parameter4.AsString())
                dbVal4 = parameter4.AsString()
                for parameter5 in selectedElement.GetParameters("FO_Number"):
                    value5.append(parameter5.AsString())
                    dbVal5 = parameter5.AsString()
                    for parameter6 in selectedElement.GetParameters("M_Level"):
                        value6.append(parameter6.AsString())
                        dbVal6 = parameter6.AsString()
                        for parameter7 in selectedElement.GetParameters("Quantity"):
                            value7.append(parameter7.AsString())
                            dbVal7 = parameter7.AsString()
                            for parameter8 in selectedElement.GetParameters("Status"):
                                value8.append(parameter8.AsString())
                                dbVal8 = parameter8.AsString()
                            query = "UPDATE %s SET Mark = '%s',Description = '%s',Tag_Number = '%s',Type_Assembly = '%s',FO_Number = '%s',M_Level = '%s',Quantity = '%s',Status = '%s' WHERE %s.ELEMENT_ID = '%s' " % (table, dbVal, dbVal2, dbVal3, dbVal4, dbVal5, dbVal6, dbVal7, dbVal8, table, selectedElementId)
                            adaptor = OdbcDataAdapter(query, connection)
                            dataSet = DataSet()
                            adaptor.Fill(dataSet)

        # Commit transaction
       

# connection.Close()
