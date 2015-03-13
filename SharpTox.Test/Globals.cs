using SharpTox.Core;

namespace SharpTox.Test
{
    static class Globals
    {
        public static ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("178.62.250.138", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B")),
            new ToxNode("192.210.149.121", 33445, new ToxKey(ToxKeyType.Public, "F404ABAA1C99A9D37D61AB54898F56793E1DEF8BD46B1038B9D822E8460FAB67")),
            new ToxNode("178.62.125.224", 33445, new ToxKey(ToxKeyType.Public, "10B20C49ACBD968D7C80F2E8438F92EA51F189F4E70CFBBB2C2C8C799E97F03E")),
            new ToxNode("76.191.23.96", 33445, new ToxKey(ToxKeyType.Public, "93574A3FAB7D612FEA29FD8D67D3DD10DFD07A075A5D62E8AF3DD9F5D0932E11")),
        };

        public static ToxNode[] TcpRelays = new ToxNode[]
        {
            new ToxNode("178.62.250.138", 443, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B")),
            new ToxNode("104.219.184.206", 443, new ToxKey(ToxKeyType.Public, "8CD087E31C67568103E8C2A28653337E90E6B8EDA0D765D57C6B5172B4F1F04C")),
            new ToxNode("194.249.212.109", 443, new ToxKey(ToxKeyType.Public, "3CEE1F054081E7A011234883BC4FC39F661A55B73637A5AC293DDF1251D9432B"))
        };
    }
}
