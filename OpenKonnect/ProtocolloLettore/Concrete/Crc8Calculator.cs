using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKonnect.ProtocolloLettore.Concrete
{
    public class Crc8Calculator
    {
        private readonly byte[] table = new byte[256];

        public byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = table[crc ^ b];
                }
            }
            byte result = (byte)(crc ^ 0x01);

            if (result == 1)
                return 2;
            else if (result == 0x0d)
                return 0x0e;
            else
                return result;
        }

        public Crc8Calculator()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ 0x01;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }
    }
}
