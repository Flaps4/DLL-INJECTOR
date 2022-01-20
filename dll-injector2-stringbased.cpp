#include <iostream>
#include <Windows.h>
#include <TlHelp32.h>

//Proc ID function
//Getting process name 
DWORD GetProcID(const char* processName) {
    DWORD processId = 0;
    //Get a snapshot
    //getting snapshot to get the process name and return it

    //creating snapshot from processes, makes sure that process is not null
    HANDLE hsnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

    //Check if its a good snapshot or not/ that its not null or -1
    if (hsnap != INVALID_HANDLE_VALUE)
    {
        //creating a proc entry
        PROCESSENTRY32 processEntry;

        //Proc entry reciving each process entry from snapshot taken we get the first procentry by calling Process32First
        processEntry.dwSize = sizeof(processEntry);

        if (Process32First(hsnap, &processEntry))
        {
            do
            {
                //String compare | makes sure if you put in capital char it still detects your process name
                if (!_stricmp(processEntry.szExeFile, processName))
                {
                    //When we find our process name/id we break and close handle
                    processId = processEntry.th32ProcessID;
                    break;

                }
                //looping throug all processes
            } while (Process32Next(hsnap, &processEntry));
        }

    }
    //Closing handle after if-while loop is done
    CloseHandle(hsnap);
    return(processId);
}

int main()
{
    const char* dllPath = "C:\\Users\\Steam\\Desktop\\rdy.dll";
    const char* processName = "ReadyOrNot-Win64-Shipping.exe";
    DWORD processID = 0;
    
    while (!processID)
    {
        processID = GetProcID(processName);
        Sleep(30);
    }

    //calling openprocess with process all access bc we want read and write premissions
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, 0, processID);

    //If process is not null and not invalid handle
    if (hProc && hProc != INVALID_HANDLE_VALUE)
    {
        //calling virtualallocex which is gonna allocate memory in an external process,
        //It knows what process by hsnap/handle, we need max memory for a string.
        //Mem commit is real commited memory or mem_reserve is reserved memory
        //Then we need read and write permissions

        //this is going to get a section of memory in the target process, then allocate the memory and give us read and write permissions
        //and then writeprocess memory the path which is "C:\\Users\\Steam\\Desktop\\rdy.dll"

        //we need the path in the memory because we are creating a remote thread, and is going to call loadlibrary A, the paramater is of the loadlibrary is "loc/location"
        // the "loc" is the location that we wrote our DLL path in


        void* loc = VirtualAllocEx(hProc, 0, MAX_PATH, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

        if (loc) {
            WriteProcessMemory(hProc, loc, dllPath, strlen(dllPath) + 1, 0);
        }

        WriteProcessMemory(hProc, loc, dllPath, strlen(dllPath) + 1, 0);
        
        //LPTHREAD IS A WINAPI
        HANDLE hThread = CreateRemoteThread(hProc, 0, 0, (LPTHREAD_START_ROUTINE)LoadLibraryA, loc, 0, 0);
        if (hThread)
        {
            CloseHandle(hThread);
        }

    }
        if (hProc)
        {
            CloseHandle(hProc);
        }
        return 0;
}
