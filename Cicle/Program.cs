using System;
using PADI_DSTM;

internal class Cicle
{
    private static void Main(string[] args)
    {
        bool res;

        PadiDstm.Init();

        if ((args.Length > 0) && (args[0].Equals("C")))
        {
            res = PadiDstm.TxBegin();
            PadInt pi_a = PadiDstm.CreatePadInt(2);
            PadInt pi_b = PadiDstm.CreatePadInt(2000000001);
            PadInt pi_c = PadiDstm.CreatePadInt(1000000000);
            pi_a.Write(0);
            pi_b.Write(0);
            res = PadiDstm.TxCommit();
        }
        Console.WriteLine("####################################################################");
        Console.WriteLine("Finished creating PadInts. Press enter for 300 R/W transaction cycle.");
        Console.WriteLine("####################################################################");
        Console.ReadLine();
        for (int i = 0; i < 300; i++)
        {
            res = PadiDstm.TxBegin();
            PadInt pi_d = PadiDstm.AccessPadInt(2);
            PadInt pi_e = PadiDstm.AccessPadInt(2000000001);
            PadInt pi_f = PadiDstm.AccessPadInt(1000000000);
            int d = pi_d.Read();
            d++;
            pi_d.Write(d);
            int e = pi_e.Read();
            e++;
            pi_e.Write(e);
            int f = pi_f.Read();
            f++;
            pi_f.Write(f);
            Console.Write(".");
            res = PadiDstm.TxCommit();
            if (!res) Console.WriteLine("$$$$$$$$$$$$$$ ABORT $$$$$$$$$$$$$$$$$");
        }
        Console.WriteLine("####################################################################");
        Console.WriteLine("Status after cycle. Press enter for verification transaction.");
        Console.WriteLine("####################################################################");
        PadiDstm.Status();
        Console.ReadLine();
        res = PadiDstm.TxBegin();
        PadInt pi_g = PadiDstm.AccessPadInt(2);
        PadInt pi_h = PadiDstm.AccessPadInt(2000000001);
        PadInt pi_j = PadiDstm.AccessPadInt(1000000000);
        int g = pi_g.Read();
        int h = pi_h.Read();
        int j = pi_j.Read();
        res = PadiDstm.TxCommit();
        Console.WriteLine("####################################################################");
        Console.WriteLine("2 = " + g);
        Console.WriteLine("2000000001 = " + h);
        Console.WriteLine("1000000000 = " + j);
        Console.WriteLine("Status post verification transaction. Press enter for exit.");
        Console.WriteLine("####################################################################");
        PadiDstm.Status();
        Console.ReadLine();
    }
}