using UnityEngine;
using System;

namespace TerraUnity.Edittime
{
    public class map_pixel_class
    {
        public double x;
        public double y;

        void reset()
        {
            x = 0;
            y = 0;
        }
    }

    public static class TConvertors
    {
        public const double _minLatitude = -85.05112878;
        public const double _maxLatitude = 85.05112878;
        public const double pi = 3.14159265358979323846264338327950288419716939937510;

        public static latlong_class clip_latlong(latlong_class latlong, double maxLatitude= _maxLatitude, double minLatitude = _minLatitude)
        {
            if (latlong.latitude > maxLatitude)
                latlong.latitude -= (maxLatitude * 2);
            else if (latlong.latitude < minLatitude)
                latlong.latitude += (maxLatitude * 2);

            if (latlong.longitude > 180)
                latlong.longitude -= 360;
            else if (latlong.longitude < -180)
                latlong.longitude += 360;

            return latlong;
        }

        public static map_pixel_class clip_pixel(map_pixel_class map_pixel, double zoom)
        {
            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            if (map_pixel.x > mapSize - 1)
                map_pixel.x -= mapSize - 1;
            else if (map_pixel.x < 0)
                map_pixel.x = mapSize - 1 - map_pixel.x;

            if (map_pixel.y > mapSize - 1)
                map_pixel.y -= mapSize - 1;
            else if (map_pixel.y < 0)
                map_pixel.y = mapSize - 1 - map_pixel.y;

            return map_pixel;
        }

        public static Vector2 latlong_to_pixel(latlong_class latlong, latlong_class latlong_center, double zoom, Vector2 screen_resolution, double maxLatitude = _maxLatitude, double minLatitude = _minLatitude)
        {
            latlong = clip_latlong(latlong, maxLatitude,  minLatitude);
            latlong_center = clip_latlong(latlong_center, maxLatitude, minLatitude);

            double x = (latlong.longitude + 180.0) / 360.0;
            double sinLatitude = Math.Sin(latlong.latitude * pi / 180.0);
            double y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            Vector2 pixel = new Vector2((float)x, (float)y);

            x = (latlong_center.longitude + 180.0) / 360.0;
            sinLatitude = Math.Sin(latlong_center.latitude * pi / 180.0);
            y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            Vector2 pixel2 = new Vector2((float)x, (float)y);
            Vector2 pixel3 = pixel - pixel2;

            pixel3.x *= (float)(256.0 * Math.Pow(2.0, zoom));
            pixel3.y *= (float)(256.0 * Math.Pow(2.0, zoom));

            pixel3 += new Vector2(screen_resolution.x / 2f, screen_resolution.y / 2f);

            return pixel3;
        }

        public static map_pixel_class latlong_to_pixel2(latlong_class latlong, double zoom, double maxLatitude = _maxLatitude, double minLatitude = _minLatitude)
        {
            latlong = clip_latlong(latlong,  maxLatitude,  minLatitude);

            double x = (latlong.longitude + 180.0) / 360.0;
            double sinLatitude = Math.Sin(latlong.latitude * pi / 180.0);
            double y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            x *= 256.0 * Math.Pow(2.0, zoom);
            y *= 256.0 * Math.Pow(2.0, zoom);

            map_pixel_class map_pixel = new map_pixel_class();

            map_pixel.x = x;
            map_pixel.y = y;

            return map_pixel;
        }

        public static latlong_class pixel_to_latlong2(map_pixel_class map_pixel, double zoom)
        {
            map_pixel = clip_pixel(map_pixel, zoom);

            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            double x = (map_pixel.x / mapSize) - 0.5;
            double y = 0.5 - (map_pixel.y / mapSize);

            latlong_class latlong = new latlong_class();

            latlong.latitude = 90.0 - 360.0 * Math.Atan(Math.Exp(-y * 2.0 * pi)) / pi;
            latlong.longitude = 360.0 * x;

            return latlong;
        }

        public static latlong_class pixel_to_latlong(Vector2 offset, latlong_class latlong_center, double zoom, double maxLatitude = _maxLatitude, double minLatitude = _minLatitude)
        {
            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            map_pixel_class map_pixel_center = latlong_to_pixel2(latlong_center, zoom);
            map_pixel_class map_pixel = new map_pixel_class();

            map_pixel.x = map_pixel_center.x + offset.x;
            map_pixel.y = map_pixel_center.y + offset.y;

            double x = (map_pixel.x / mapSize) - 0.5;
            double y = 0.5 - (map_pixel.y / mapSize);

            latlong_class latlong = new latlong_class();

            latlong.latitude = 90.0 - 360.0 * Math.Atan(Math.Exp(-y * 2.0 * pi)) / pi;
            latlong.longitude = 360.0 * x;

            latlong = clip_latlong(latlong,  maxLatitude,  minLatitude);
            return latlong;
        }

        // Source: https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // Takes a lat/lon coordinate and returns Top Left coordinate of the tile
        public static int[] WorldToTilePos(double lon, double lat, int zoom)
        {
            int[] rowCol = new int[2];
            rowCol[0] = (int)Math.Floor((lon + 180.0) / 360.0 * (1 << zoom));
            rowCol[1] = (int)Math.Floor((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return rowCol;
        }

        public static double[] TileToWorldPos (int tile_x, int tile_y, int zoom)
        {
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));
            double xxx = (tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0;
            double yyy = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
            double[] point = new double[2] { xxx, yyy };

            return point;
        }
    }
}

