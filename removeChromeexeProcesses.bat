REM How do you kill Multiple Tasks at once?
REM Open CMD.
REM Type tasklist to display all running process on your computer.
REM To kill a specific process group.
REM Type taskkill /F /IM iexplore.exe (Explanation: taskkill /F {force} /IM {Image Name} {process name})

taskkill /F /IM chrome.exe
taskkill /F /IM conhost.exe
