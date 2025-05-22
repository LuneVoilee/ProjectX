namespace Map
{
    public enum HexEdgeType
    {
        //平地
        Flat,

        //斜坡
        Slope,

        //悬崖
        Cliff
    }

    public enum HexDirection
    {
        //东北
        NE,

        //东
        E,

        //东南
        SE,

        //西南
        SW,

        //西
        W,

        //西北
        NW
    }
    
    public static class HexDirectionExtensions
    {
        public static HexDirection Opposite(this HexDirection direction)
        {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }

        public static HexDirection Previous(this HexDirection direction)
        {
            return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
        }

        public static HexDirection Next(this HexDirection direction)
        {
            return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
        }
    }
}