﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;

namespace SystemTextJsonSamples
{
    public class RoundtripImmutableStruct
    {
        public static void Run()
        {
            string jsonString;
            var point1 = new ImmutablePoint(1, 2);
            var point2 = new ImmutablePoint(3, 4);
            var points = new List<ImmutablePoint> { point1, point2 };

            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.WriteIndented = true;
            //serializeOptions.Converters.Add(new WeatherForecastRequiredPropertyConverter());
            jsonString = JsonSerializer.Serialize(points, serializeOptions);
            Console.WriteLine($"JSON output:\n{jsonString}\n");

            var deserializeOptions = new JsonSerializerOptions();
            deserializeOptions.Converters.Add(new ImmutablePointConverter());
            points = JsonSerializer.Deserialize<List<ImmutablePoint>>(jsonString, deserializeOptions);
            Console.WriteLine("Deserialized object values");
            foreach (ImmutablePoint point in points)
            {
                Console.WriteLine($"X,Y = {point.X},{point.Y}");
            }
        }
    }
   
}
