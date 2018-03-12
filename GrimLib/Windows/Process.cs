using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrimLib.Windows
{
    class Process
    {
        /// <summary>
        /// WinAPI constant to read process' memory
        /// </summary>
        const int PROCESS_WM_READ = 0x0010;

        /// <summary>
        /// Private string to know the game process' name
        /// </summary>
        private string processName;

        /// <summary>
        /// Game's process
        /// </summary>
        private System.Diagnostics.Process gameProcess;

        /// <summary>
        /// Game's process' HANDLE
        /// </summary>
        private IntPtr gameProcessHandle;

        /// <summary>
        /// Low level function to get process' handle
        /// </summary>
        /// <param name="dwDesiredAccess">Desired level of access</param>
        /// <param name="bInheritHandle"></param>
        /// <param name="dwProcessId">Process id</param>
        /// <returns>Process HANDLE</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        /// <summary>
        /// Low level function to read process memory
        /// </summary>
        /// <param name="hProcess">HANDLE of a process</param>
        /// <param name="lpBaseAddress">An address to read</param>
        /// <param name="lpBuffer">Buffer to read into</param>
        /// <param name="dwSize">Number of bytes to read</param>
        /// <param name="lpNumberOfBytesRead">Total ammount of bytes that were read</param>
        /// <returns>BOOL?</returns>
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private bool isOpen = false;
        private object lockObj = new object();


        /// <summary>
        /// Is process opened
        /// </summary>
        public bool IsOpen
        {
            get
            {
                lock (lockObj)
                {
                    if (isOpen)
                    {
                        isOpen = !gameProcess.HasExited;
                        if (!isOpen)
                            OpenProcessAsync(processName);
                    }
                    return isOpen;
                }
            }
        }

        /*public static async Task<GameProcess> OpenGameProcessAsync(string processName)
        {
            return await Task.Run(() => OpenGameProcess(processName));
        }*/

        public static Process OpenGameProcess(string processName)
        {
            return new Process(processName);
        }

        private void OpenProcess()
        {
            System.Diagnostics.Process[] processes;
            do
            {
                Thread.Sleep(200);
            } while ((processes = System.Diagnostics.Process.GetProcessesByName(processName)).Length < 1);
            lock (lockObj)
            {
                gameProcess = processes[0];
                gameProcessHandle = OpenProcess(PROCESS_WM_READ, false, gameProcess.Id);
                isOpen = true;
            }
        }

        private async void OpenProcessAsync(string name)
        {
            processName = name;
            await Task.Run(() => OpenProcess());
        }

        private Process(string name)
        {
            OpenProcessAsync(name);
        }

        #region Data getters within game memory

        /// <summary>
        /// Try readign data at address
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <param name="size">Number of bytes to be read</param>
        /// <param name="data">Where to put read data</param>
        /// <param name="read">How many bytes were actually read</param>
        /// <returns>True if succedes reading at least 1 byte, false othervise</returns>
        public bool TryRead(int address, int size, byte[] data, ref int read)
        {
            if (!IsOpen)
                return false;
            bool result = ReadProcessMemory((int)gameProcessHandle, address, data, size, ref read);
            return result;
        }

        /// <summary>
        /// Try reading a pointer path
        /// </summary>
        /// <param name="size">Number of bytes to read</param>
        /// <param name="read">How many bytes were actually read</param>
        /// <param name="ret">Where to put read data</param>
        /// <param name="address">Address to read at</param>
        /// <param name="offsets">Pointer offsets</param>
        /// <returns>True if succedes reading at least 1 byte, false othervise</returns>
        public bool TryReadPath(int size, ref int read, byte[] ret, int address, int[] offsets)
        {
            if (!IsOpen)
                return false;
            byte[] data = new byte[4];
            bool t = TryRead(address, 4, data, ref read);
            if (!t)
                return t;

            int baseAddr = BitConverter.ToInt32(data, 0);
            for (int i = 0; i < offsets.Length; i++)
            {
                t = TryRead(baseAddr + offsets[i], 4, data, ref read);
                if (!t)
                    return t;
                baseAddr = BitConverter.ToInt32(data, 0);
            }
            t = TryRead(baseAddr, size, ret, ref read);
            return t;
        }

        /// <summary>
        /// Read pointer path
        /// </summary>
        /// <param name="size">Number of bytes to read</param>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public byte[] ReadPointerPath(int size, int baseAddr, params int[] offsets)
        {
            int read = 0;
            byte[] data = new byte[size];
            bool t = TryReadPath(size, ref read, data, baseAddr, offsets);
            if (!t && read != size)
                throw new Exception("Memory read error");
            return data;
        }


        /*public bool TryReadMemory(int address, int size, byte[] data)
        {
            int read = 0;
            return TryRead(address, size, data, ref read);
        }*/

        /// <summary>
        /// Read process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <param name="size">Number of bytes read</param>
        /// <returns></returns>
        public byte[] ReadMemory(int address, int size)
        {
            int read = 0;
            byte[] data = new byte[size];
            bool t = TryRead(address, size, data, ref read);
            if(!t || read < size)
                throw new Exception("Memory read error");

            return data;
        }

        /// <summary>
        /// Read an int from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public int ReadInt32(int address)
        {
            return BitConverter.ToInt32(ReadMemory(address, 4), 0);
        }

        /// <summary>
        /// Read a float from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public float ReadFloat(int address)
        {
            return BitConverter.ToSingle(ReadMemory(address, 4), 0);
        }

        /// <summary>
        /// Read a double from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public double ReadDouble(int address)
        {
            return BitConverter.ToDouble(ReadMemory(address, 8), 0);
        }
        /// <summary>
        /// Read a short from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public short ReadInt16(int address)
        {
            return BitConverter.ToInt16(ReadMemory(address, 2), 0);
        }

        /// <summary>
        /// Read a byte from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public byte ReadByte(int address)
        {
            return ReadMemory(address, 1)[0];
        }

        /// <summary>
        /// Read a long from process memory
        /// </summary>
        /// <param name="address">Address to read at</param>
        /// <returns></returns>
        public long ReadInt64(int address)
        {
            return BitConverter.ToInt32(ReadMemory(address, 8), 0);
        }

        /// <summary>
        /// Read a pointer path to an int from process memory
        /// </summary>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public int ReadInt32Path(int baseAddr, params int[] offsets)
        {
            return BitConverter.ToInt32(ReadPointerPath(4, baseAddr, offsets), 0);
        }

        /// <summary>
        /// Read a pointer path to a float from process memory
        /// </summary>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public float ReadFloatPath(int baseAddr, params int[] offsets)
        {
            return BitConverter.ToSingle(ReadPointerPath(4, baseAddr, offsets), 0);
        }

        /// <summary>
        /// Read a pointer path to a short from process memory
        /// </summary>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public short ReadInt16Path(int baseAddr, params int[] offsets)
        {
            return BitConverter.ToInt16(ReadPointerPath(2, baseAddr, offsets), 0);
        }

        /// <summary>
        /// Read a pointer path to a long from process memory
        /// </summary>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public long ReadInt64Path(int baseAddr, params int[] offsets)
        {
            return BitConverter.ToInt64(ReadPointerPath(8, baseAddr, offsets), 0);
        }

        /// <summary>
        /// Read a pointer path to a double from process memory
        /// </summary>
        /// <param name="baseAddr">Address to read at</param>
        /// <param name="offsets">Pointer path offsets</param>
        /// <returns></returns>
        public double ReadDoublePath(int baseAddr, params int[] offsets)
        {
            return BitConverter.ToDouble(ReadPointerPath(8, baseAddr, offsets), 0);
        }

        #endregion
    }
}
