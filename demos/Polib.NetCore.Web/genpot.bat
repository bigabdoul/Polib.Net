rem Extracts translation strings from source files and creates corresponding .pot translation template files.

rem Check if temp directory exist; if not, create it
IF NOT EXIST temp mkdir temp

cd src\Controllers
dir /S /B *.cs > ..\..\temp\Controllers.filelist
cd ..\Models
dir /S /B *.cs > ..\..\temp\Models.filelist
cd ..\Views
dir /S /B *.cshtml > ..\..\temp\Views.filelist
cd ..\..

xgettext.exe -k -kT -kTn:1,2 -kTx:1c,2 -kTxn:1c,2,3 --from-code=UTF-8 -LC# --omit-header -otemp\Controllers.pot -ftemp\Controllers.filelist
xgettext.exe -k -kT -kTn:1,2 -kTx:1c,2 -kTxn:1c,2,3 --from-code=UTF-8 -LC# --omit-header -otemp\Models.pot -ftemp\Models.filelist
xgettext.exe -k -kT -kTn:1,2 -kTx:1c,2 -kTxn:1c,2,3 -kH -kHn:1,2 -kHx:1c,2 -kHxn:1c,2,3 --from-code=UTF-8 -LC# --omit-header -otemp\Views.pot -ftemp\Views.filelist
