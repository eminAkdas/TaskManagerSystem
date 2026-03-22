using System;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

public class Program {
    public static void Main() {
        var services = new ServiceCollection();
        // try to find AddAutoMapper extensions
        foreach(var m in typeof(AutoMapper.ServiceCollectionExtensions).GetMethods()) {
            Console.WriteLine(m.ToString());
        }
    }
}
