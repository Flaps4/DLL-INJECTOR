using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace DLL_INJECTOR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            comboBox1.Items.Clear();
            foreach (Process p in PC) {
                comboBox1.Items.Add(p.ProcessName);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DLLPATH = textBox1.Text;
        }
        private static string DLLPATH { get; set;}

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                OpenFileDialog OFD = new OpenFileDialog();
                OFD.InitialDirectory = @"C:\";
                OFD.Title = "Click to Locate DLL file for Injection";
                OFD.DefaultExt = "dll";
                OFD.Filter = "DLL Files (*.dll) | *.dll";
                OFD.CheckFileExists = true;
                OFD.CheckPathExists = true;
                OFD.ShowDialog();
                textBox1.Text = OFD.FileName;
                DLLPATH = OFD.FileName;
            }
            catch (Exception error) {
                MessageBox.Show(error.Message);
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            comboBox1.Items.Clear();
            foreach (Process p in PC)
            {
                comboBox1.Items.Add(p.ProcessName);
            }
        }

        //Reads INTPTR
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;
        //Indicates when is called if its setting an last error berfre returning from the attributed method
        [DllImport("kernel32.dll", SetLastError = true)]

        //Opens choses process and gets process id
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        
        //Closing the chooses object(procces) handle
        static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]


        //gettin g process adress/name, 
        static extern IntPtr GetProcAdress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", SetLastError = true)]

        //allocation memory in address space of another process
        static extern IntPtr VirtualAllocationEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]

        //Write data to memory of a set process
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]

        //Creating an remote thread that runs in virtual adress space of another process
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParamater, uint dwCreationFlags, IntPtr lpThreadId);

        public static int Inject(string ProcName, string DLLPATH)
        {
            //0= file exist 1= file dont exist
            //2 process not acticve
            //3 injection failed
            //4 injection succseded
            if (!File.Exists(DLLPATH)) { return 1; }
            uint _procId = 0;
            //If process name = process that we want then we know that the process is active and know the process id
            Process[] _procs = Process.GetProcesses();
            for (int i = 0; i < _procs.Length; i++)
            {
                if (_procs[i].ProcessName == ProcName)
                {
                    _procId = (uint)_procs[i].Id;
                }
            }
            // process check if it exists 
            if (_procId == 0) { return 2; }

            if (!StartInjection(_procId, DLLPATH))
            {
                return 3;
            }

            return 4;

        }
        //injection thread

        public static bool StartInjection(uint P, string DLLPATH) 
        {
            IntPtr handleProcess = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, P);

            //Return false if injection fails
            if (handleProcess == INTPTR_ZERO) {
                return false;
            }

            IntPtr lpAddress = VirtualAllocationEx(handleProcess, (IntPtr)null, (IntPtr)DLLPATH.Length, (0x1000 | 0x2000), 0x40);

            if (lpAddress == INTPTR_ZERO)
            {
                return false;
            }

            //getting bytes of process by doing byte array
            byte[] bytes = Encoding.ASCII.GetBytes(DLLPATH);

            //Write proccess that we got from kernel32 earlier
            if (WriteProcessMemory(handleProcess, lpAddress, bytes, (uint)bytes.Length, 0) == 0)
            {
                return false;
            }

            //Successfull injection
            CloseHandle(handleProcess);

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int Result = Inject(comboBox1.Text, DLLPATH);
            if (Result == 1)
            {
                MessageBox.Show("DLL filen finns tyvär ej");
            }
            if (Result == 2)
            {
                MessageBox.Show("Processes finns ej eller är inte aktiv");
            }
            if (Result == 3)
            {
                MessageBox.Show("Injektion misslyckades");
            }
            if (Result == 4)
            {
                MessageBox.Show("Injektion Lyckades");
            }
        }
    }
}
