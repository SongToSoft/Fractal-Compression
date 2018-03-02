using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using Cloo;
using OpenCLTemplate;

using System.Runtime.InteropServices;

namespace NewFractalCompression.Code
{
    class OpenTK_test
    {
        static public void StartTestCLT()
        {
            //Текст програмы, исполняющейся на устройстве (GPU или CPU). Именно эта программа будет выполнять паралельные
            //вычисления и будет складывать вектора. Программа написанна на языке, основанном на C99 специально под OpenCL.
            //string vecSum = @"
            //       __kernel void floatVectorSum(__global float * v1, __global float * v2)
            //       {
            //            int i = get_global_id(0);
            //            v1[i] = v1[i] + v2[i];
            //       }
            //       ";

            //Инициализация платформы. В скобках можно задавать параметры. По умолчанию инициализируются только GPU.
            //OpenCLTemplate.CLCalc.InitCL(Cloo.ComputeDeviceTypes.All) позволяет инициализировать не только GPU но и CPU.
            OpenCLTemplate.CLCalc.InitCL();
            //Команда выдаёт список проинициализированных устройств.
            List<Cloo.ComputeDevice> L = OpenCLTemplate.CLCalc.CLDevices;
            //Команда устанавливает устройство с которым будет вестись работа
            OpenCLTemplate.CLCalc.Program.DefaultCQ = 0;
            //Компиляция программы vecSum
            OpenCLTemplate.CLCalc.Program.Compile(new string[] { StrFunc.vecSum });
            //Присовоение названия скомпилированной программе, её загрузка.
            OpenCLTemplate.CLCalc.Program.Kernel VectorSum = new OpenCLTemplate.CLCalc.Program.Kernel("floatVectorSum");
            int n = 10;
            float[] v1 = new float[n], v2 = new float[n], v3 = new float[n];
            //Инициализация и присвоение векторов, которые мы будем складывать.
            for (int i = 0; i < n; i++)
            {
                v1[i] = i;
                v2[i] = i * 2;
            }
            //Загружаем вектора в память устройства
            OpenCLTemplate.CLCalc.Program.Variable varV1 = new OpenCLTemplate.CLCalc.Program.Variable(v1);
            OpenCLTemplate.CLCalc.Program.Variable varV2 = new OpenCLTemplate.CLCalc.Program.Variable(v2);
            //Объявление того, кто из векторов кем является
            OpenCLTemplate.CLCalc.Program.Variable[] args = new OpenCLTemplate.CLCalc.Program.Variable[] { varV1, varV2 };
            //Сколько потоков будет запущенно
            int[] workers = new int[1] { n };
            //Исполняем ядро VectorSum с аргументами args и колличеством потоков workers
            VectorSum.Execute(args, workers);
            //выгружаем из памяти
            varV1.ReadFromDeviceTo(v1);
            for (int i = 0; i < v1.Length; ++i)
            {
                System.Console.Write(v1[i]);
                System.Console.Write(" ");
            }
        }
        static public void StartTestCL()
        {
            //Установка параметров, инициализирующих видеокарты при работе. В Platforms[1] должен стоять индекс
            //указывающий на используемую платформу
            //System.Console.WriteLine(ComputePlatform.Platforms[0]);

            //Меняю все ComputePlatform.Platforms[1] на ComputePlatform.Platforms[0]
            ComputeContextPropertyList Properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
            ComputeContext Context = new ComputeContext(ComputeDeviceTypes.All, Properties, null, IntPtr.Zero);

            //Текст програмы, исполняющейся на устройстве(GPU или CPU).Именно эта программа будет выполнять паралельные
            //вычисления и будет складывать вектора. Программа написанна на языке, основанном на C99 специально под OpenCL.
            string vecSum = @" 
                    __kernel void floatVectorSum(__global float * v1, __global float * v2)
                    {
                        int i = get_global_id(0);
                        v1[i] = v1[i] + v2[i];
                    }
                    ";
            //Список устройств, для которых мы будем компилировать написанную в vecSum программу
            List<ComputeDevice> Devs = new List<ComputeDevice>();
            Devs.Add(ComputePlatform.Platforms[0].Devices[0]);
            Devs.Add(ComputePlatform.Platforms[0].Devices[1]);
            Devs.Add(ComputePlatform.Platforms[0].Devices[2]);
            //Компиляция программы из vecSum
            ComputeProgram prog = null;
            try
            {

                //prog = new ComputeProgram(Context, Str.vecSum);
                prog = new ComputeProgram(Context, StrFunc.vecSum);
                prog.Build(Devs, "", null, IntPtr.Zero);
            }
            catch
            {

            }
            //Инициализация новой программы
            ComputeKernel kernelVecSum = prog.CreateKernel("floatVectorSum");
            //Инициализация и присвоение векторов, которые мы будем складывать.
            int n = 10;
            float[] v1 = new float[n], v2 = new float[n];
            for (int i = 0; i < v1.Length; i++)
            {
                v1[i] = i;
                v2[i] = i * 2;
            }
            //Загрузка данных в указатели для дальнейшего использования.
            ComputeBuffer<float> bufV1 = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, v1);
            ComputeBuffer<float> bufV2 = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, v2);
            //Объявляем какие данные будут использоваться в программе vecSum
            kernelVecSum.SetMemoryArgument(0, bufV1);
            kernelVecSum.SetMemoryArgument(1, bufV2);
            //Создание програмной очереди. Не забудте указать устройство, на котором будет исполняться программа!
            ComputeCommandQueue Queue = new ComputeCommandQueue(Context, Cloo.ComputePlatform.Platforms[0].Devices[0], Cloo.ComputeCommandQueueFlags.None);
            //Старт. Execute запускает программу-ядро vecSum указанное колличество раз (v1.Length)
            Queue.Execute(kernelVecSum, null, new long[] { v1.Length }, null, null);
            //Считывание данных из памяти устройства.
            float[] arrC = new float[n];
            GCHandle arrCHandle = GCHandle.Alloc(arrC, GCHandleType.Pinned);
            Queue.Read<float>(bufV1, true, 0, n, arrCHandle.AddrOfPinnedObject(), null);
            for (int i = 0; i < v1.Length; i++)
            {
                System.Console.Write(arrC[i]);
                System.Console.Write(" ");
            }
        }
        static public void StartTest()
        {
            float[] v1 = new float[100], v2 = new float[100];
            for (int i = 0; i < v1.Length; i++)
            {
                v1[i] = i;
                v2[i] = 2 * i;
            }
            for (int i = 0; i < v1.Length; ++i)
            {
                v1[i] = v1[i] + v2[i];
            }
        }
    }
}
