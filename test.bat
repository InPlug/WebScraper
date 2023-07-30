set PFILES=%PROGRAMFILES: (x86)=%&& wmic datafile where name='!PFILES:\\=\\\\!\\\\Google\\\\Chrome\\\\Application\\\\chrome.exe' get Version /value

