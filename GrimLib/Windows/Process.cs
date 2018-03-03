﻿using System;
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

        private bool TryRead(int address, int size, byte[] data, ref int read)
        {
            if (!IsOpen)
                return false;
            bool result = ReadProcessMemory((int)gameProcessHandle, address, data, size, ref read);
            return result;
        }

        public bool TryReadMemory(int address, int size, byte[] data)
        {
            int read = 0;
            return TryRead(address, size, data, ref read);
        }

        public byte[] ReadMemory(int address, int size)
        {
            int read = 0;
            byte[] data = new byte[size];
            if(TryRead(address, size, data, ref read))
            {
                if (read < size)
                    throw new Exception("Memory read error");

                return data;
            }

            throw new Exception("Memory read error");
        }

        public int ReadInt32(int address)
        {
            return BitConverter.ToInt32(ReadMemory(address, 4), 0);
        }

        public float ReadFloat(int address)
        {
            return BitConverter.ToSingle(ReadMemory(address, 4), 0);
        }

        public double ReadDouble(int address)
        {
            return BitConverter.ToDouble(ReadMemory(address, 8), 0);
        }

        public short ReadInt16(int address)
        {
            return BitConverter.ToInt16(ReadMemory(address, 2), 0);
        }

        public byte ReadByte(int address)
        {
            return ReadMemory(address, 1)[0];
        }

        public long ReadInt64(int address)
        {
            return BitConverter.ToInt32(ReadMemory(address, 8), 0);
        }

        public int ReadInt32Path(int baseAddr, params int[] offset)
        {
            int read = 0;
            byte[] data = new byte[4];
            ReadProcessMemory((int)gameProcessHandle, baseAddr, data, 4, ref read);
            baseAddr = BitConverter.ToInt32(data, 0);
            for (int i = 0; i < offset.Length; i++)
            {
                ReadProcessMemory((int)gameProcessHandle, baseAddr + offset[i], data, 4, ref read);
                baseAddr = BitConverter.ToInt32(data, 0);
            }
            ReadProcessMemory((int)gameProcessHandle, baseAddr, data, 4, ref read);
            return BitConverter.ToInt32(data, 0);
        }

        public float ReadFloatPath(int baseAddr, params int[] offset)
        {
            int read = 0;
            byte[] data = new byte[4];
            ReadProcessMemory((int)gameProcessHandle, baseAddr, data, 4, ref read);
            baseAddr = BitConverter.ToInt32(data, 0);
            for (int i = 0; i < offset.Length; i++)
            {
                if (!ReadProcessMemory((int)gameProcessHandle, baseAddr + offset[i], data, 4, ref read)) return 0.0f;
                baseAddr = BitConverter.ToInt32(data, 0);
                if (baseAddr == 0)
                    return 0.0f;
            }
            ReadProcessMemory((int)gameProcessHandle, baseAddr, data, 4, ref read);
            return BitConverter.ToSingle(data, 0);
        }

        #endregion
    }
}
