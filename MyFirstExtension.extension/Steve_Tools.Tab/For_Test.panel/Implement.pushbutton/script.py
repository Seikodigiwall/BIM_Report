__title__ = 'TEST'
__doc__ = "."


import sys
import os
import os.path as op
import string
from collections import OrderedDict
import threading
from functools import wraps

from pyrevit import HOST_APP, EXEC_PARAMS
from pyrevit.compat import safe_strtype
from pyrevit import coreutils
from pyrevit.coreutils.logger import get_logger
from pyrevit import framework
from pyrevit.framework import System
from pyrevit.framework import Threading
from pyrevit.framework import Interop
from pyrevit.framework import wpf, Forms, Controls, Media
from pyrevit.api import AdWindows
from pyrevit import revit, UI, DB
from pyrevit import forms

ops = ['option1', 'option2', 'option3', 'option4']
forms.CommandSwitchWindow.show(ops, message='Select Option')
# count = 1
# with forms.ProgressBar(title='my command progress message') as pb: pb.update_progress(count, 100)
# count += 1
