using NModbus;
using NModbus.Serial;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Ports;

namespace ModbusTest01
{
    class Program
    {

        static int make = 2; // 0 = LICHUAN; 1 = SZGH; 2 = VFD
        static int Main(string[] args)
        {

            var cmdList = new Command("list", "Lists the available serial ports");
            var cmdRead = new Command("read", "Reads a number of registers")
            {
                new Argument<string>("portname", "The serial port to use"),
                new Argument<ushort>("startaddress", "Starting address of registers to read"),
                new Argument<ushort>("numregisters", "Number of registers to read")
            };
            var cmdWrite = new Command("write", "Writes a value to a register")
            {
                new Argument<string>("portname", "The serial port to use"),
                new Argument<ushort>("startaddress", "Register addresss"),
                new Argument<ushort>("regval", "Value to write")
            };
            var cmdReadCoils = new Command("readcoils", "Reads a range of coils")
            {
                new Argument<string>("portname", "The serial port to use"),
                new Argument<ushort>("startaddress", "Starting address of coils to read"),
                new Argument<ushort>("numcoils", "Number of coils to read")
            };
            var cmdWriteCoil = new Command("writecoil", "Writes a single coil")
            {
                new Argument<string>("portname", "The serial port to use"),
                new Argument<ushort>("startaddress", "Coil addresss"),
                new Argument<bool>("coilval", "Value to write")
            };
            var cmdReadAllRegisters = new Command("readallregisters", "reads all registers and writes them to a file")
            {
                new Argument<string>("portname", "The serial port to use"),
                new Argument<string>("filename", "Name of the file to output results to")
            };


            cmdList.Handler = CommandHandler.Create<bool, IConsole>(doListPorts);
            cmdRead.Handler = CommandHandler.Create<string, ushort, ushort>(ModbusSerialRTUMasterReadRegisters);
            cmdWrite.Handler = CommandHandler.Create<string, ushort, ushort>(ModbusSerialRtuMasterWriteRegisters);
            cmdReadCoils.Handler = CommandHandler.Create<string, ushort, ushort>(ModbusSerialRTUMasterReadCoils);
            cmdWriteCoil.Handler = CommandHandler.Create<string, ushort, bool>(ModbusSerialRtuMasterWriteCoil);
            cmdReadAllRegisters.Handler = CommandHandler.Create<string, string>(ReadAllRegisters);

            var cmd = new RootCommand
            {
                cmdList,
                cmdRead,
                cmdWrite,
                cmdReadCoils,
                cmdWriteCoil,
                cmdReadAllRegisters
            };


            return cmd.Invoke(args);

        }

        static void doListPorts(bool list, IConsole c)
        {
            ListPorts();
        }
        static void ListPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");             // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }
        }

        public static void ModbusSerialRTUMasterReadRegisters(string portname, ushort startaddress, ushort numregisters)
        {

            
            using (SerialPort port = new SerialPort(portname))
            {
                // configure serial port

                switch (make) {
                    case 0: // LICHUAN
                        port.BaudRate = 19200;
                        port.DataBits = 8;
                        port.Parity = Parity.Even;
                        port.StopBits = StopBits.One;
                        break;
                    case 1: // SZGH
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                    default: // case 2 vfd
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                }

                port.ReadTimeout = 500;
                port.Open();


                var factory = new ModbusFactory();
                IModbusSerialMaster master = factory.CreateRtuMaster(port);
                

                byte slaveId = 1;
                ushort startAddress = startaddress;
                ushort numRegisters = numregisters;
                try
                {
                    ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                    for (int i = 0; i < numRegisters; i++)
                    {
                        Console.WriteLine($"Register {startAddress + i}={registers[i]} - {registers[i]:X2}");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception reading registers: " + e.Message);
                }

            }
        }

        public static void ModbusSerialRtuMasterWriteRegisters(string portname, ushort startaddress, ushort regval)
        {

            // read the current value
            ModbusSerialRTUMasterReadRegisters(portname, startaddress, 1);
            Console.WriteLine("Writing new value: {0}", regval);

            using (SerialPort port = new SerialPort(portname))
            {
                switch (make)
                {
                    case 0: // LICHUAN
                        port.BaudRate = 19200;
                        port.DataBits = 8;
                        port.Parity = Parity.Even;
                        port.StopBits = StopBits.One;
                        break;
                    default: // SZGH
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                }

                port.ReadTimeout = 500;
                port.Open();

                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(port);

                byte slaveId = 1;
                //ushort startAddress = startaddress;
                //ushort[] registers = new ushort[] { regval };
                try
                {
                    master.WriteSingleRegister(slaveId, startaddress, regval);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"{startaddress}: {e}");
                }
            }

            // read the new value
            ModbusSerialRTUMasterReadRegisters(portname, startaddress, 1);
        }

        public static void ModbusSerialRTUMasterReadCoils(string portname, ushort startaddress, ushort numcoils)
        {


            using (SerialPort port = new SerialPort(portname))
            {
                // configure serial port

                switch (make)
                {
                    case 0: // LICHUAN
                        port.BaudRate = 19200;
                        port.DataBits = 8;
                        port.Parity = Parity.Even;
                        port.StopBits = StopBits.One;
                        break;
                    case 1: // SZGH
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                    default: // case 2 vfd
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                }

                port.ReadTimeout = 500;
                port.Open();


                var factory = new ModbusFactory();
                IModbusSerialMaster master = factory.CreateRtuMaster(port);


                byte slaveId = 1;
                ushort startAddress = startaddress;
                ushort numRegisters = numcoils;
                for (int i = 0; i < numRegisters; i++) 
                { 
                    try
                    {
                        // read five registers		
                        bool[] coils = master.ReadCoils(slaveId, startAddress, 1);


                        {
                            Console.WriteLine($"Coil {startAddress + i} = {coils[0]}");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception reading coil {startAddress+i}: " + e.Message);
                    }
                }

            }

        }

        public static void ModbusSerialRtuMasterWriteCoil(string portname, ushort startaddress, bool coilval)
        {

            // read the current value
            ModbusSerialRTUMasterReadCoils(portname, startaddress, 1);
            Console.WriteLine("Writing new value: {0}", coilval);

            using (SerialPort port = new SerialPort(portname))
            {
                switch (make)
                {
                    case 0: // LICHUAN
                        port.BaudRate = 19200;
                        port.DataBits = 8;
                        port.Parity = Parity.Even;
                        port.StopBits = StopBits.One;
                        break;
                    default: // SZGH
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                }

                port.ReadTimeout = 500;
                port.Open();

                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(port);

                byte slaveId = 1;
                //ushort startAddress = startaddress;
                //ushort[] registers = new ushort[] { regval };
                try
                {
                    master.WriteSingleCoil(slaveId, startaddress, coilval);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception writing coil: " + e.ToString());
                }
            }

            // read the new value
            ModbusSerialRTUMasterReadCoils(portname, startaddress, 1);
        }

        public static void ReadAllRegisters(string portname, string filename)
        {

            StreamWriter file = new System.IO.StreamWriter(filename, false);
            file.WriteLine("Register\tValue");

            using (SerialPort port = new SerialPort(portname))
            {
                // configure serial port

                switch (make)
                {
                    case 0: // LICHUAN
                        port.BaudRate = 19200;
                        port.DataBits = 8;
                        port.Parity = Parity.Even;
                        port.StopBits = StopBits.One;
                        break;
                    case 1: // SZGH
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                    default: // case 2 vfd
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;

                        break;
                }

                port.ReadTimeout = 500;
                port.Open();


                var factory = new ModbusFactory();
                IModbusSerialMaster master = factory.CreateRtuMaster(port);


                byte slaveId = 1;
                ushort startAddress = 0;
                ushort numRegisters = 0xFFFF;

                bool success = false;
                ushort[] registers = new ushort[1];

                for (ushort i = 0; i < numRegisters; i++)
                {
                    try
                    {
                        // read five registers		
                        registers = master.ReadHoldingRegisters(slaveId, (ushort)(startAddress + i), 1);
                        success = true;
                                            

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0}: Exception reading registers: {1}", i, e.Message);
                        success = false;
                    }

                    if (success)
                    {
                        Console.WriteLine($"Register {i} : {registers[0]}");
                        file.WriteLine($"{i}\t{registers[0]}");
                    }
                }
            }

            file.Flush();
            file.Close();
            file.Dispose();
        }
    }
}
