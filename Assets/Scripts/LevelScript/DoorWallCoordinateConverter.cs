using System.Collections.Generic;

public static class DoorWallCoordinateConverter
{
    public static DoorSpawnData ConvertForSave(DoorSpawnData door)
    {
        return new DoorSpawnData
        {
            row = door.direction switch
            {
                "Up"    => door.row + 1,
                "Down"  => door.row - 1,
                _       => door.row
            },
            column = door.direction switch
            {
                "Left"  => door.column + 1,
                "Right" => door.column - 1,
                _       => door.column
            },
            direction = door.direction,
            doorType  = door.doorType,
            color     = door.color
        };
    }

    public static DoorSpawnData ConvertForLoad(DoorSpawnData door)
    {
        return new DoorSpawnData
        {
            row = door.direction switch
            {
                "Up"    => door.row - 1,
                "Down"  => door.row + 1,
                _       => door.row
            },
            column = door.direction switch
            {
                "Left"  => door.column - 1,
                "Right" => door.column + 1,
                _       => door.column
            },
            direction = door.direction,
            doorType  = door.doorType,
            color     = door.color
        };
    }

    public static WallSpawnData ConvertForSave(WallSpawnData wall)
    {
        return new WallSpawnData
        {
            row = wall.direction switch
            {
                "Up"    => wall.row + 1,
                "Down"  => wall.row - 1,
                _       => wall.row
            },
            column = wall.direction switch
            {
                "Left"  => wall.column + 1,
                "Right" => wall.column - 1,
                _       => wall.column
            },
            direction = wall.direction
        };
    }

    public static WallSpawnData ConvertForLoad(WallSpawnData wall)
    {
        return new WallSpawnData
        {
            row = wall.direction switch
            {
                "Up"    => wall.row - 1,
                "Down"  => wall.row + 1,
                _       => wall.row
            },
            column = wall.direction switch
            {
                "Left"  => wall.column - 1,
                "Right" => wall.column + 1,
                _       => wall.column
            },
            direction = wall.direction
        };
    }
}
