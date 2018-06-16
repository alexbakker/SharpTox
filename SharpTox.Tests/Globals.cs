using SharpTox.Core;

namespace SharpTox.Test
{
    static class Globals
    {
        public static ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("node.tox.biribiri.org", 33445, new ToxKey(ToxKeyType.Public, "F404ABAA1C99A9D37D61AB54898F56793E1DEF8BD46B1038B9D822E8460FAB67")),
            new ToxNode("163.172.136.118", 33445, new ToxKey(ToxKeyType.Public, "2C289F9F37C20D09DA83565588BF496FAB3764853FA38141817A72E3F18ACA0B"))
        };

        public static ToxNode[] TcpRelays = new ToxNode[]
        {
            new ToxNode("node.tox.biribiri.org", 33445, new ToxKey(ToxKeyType.Public, "F404ABAA1C99A9D37D61AB54898F56793E1DEF8BD46B1038B9D822E8460FAB67")),
            new ToxNode("163.172.136.118", 33445, new ToxKey(ToxKeyType.Public, "2C289F9F37C20D09DA83565588BF496FAB3764853FA38141817A72E3F18ACA0B"))
        };
    }
}
