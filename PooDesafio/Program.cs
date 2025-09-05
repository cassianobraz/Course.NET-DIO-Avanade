using DesafioPOO.Models;

Console.WriteLine("Smartphone Nokia:");
Smartphone nokia = new Nokia("123456789", "Nokia 1100", "111111111111111", 512);
nokia.Ligar();
nokia.InstalarAplicativo("WhatsApp");

Console.WriteLine("\n");

Console.WriteLine("Smartphone IPhone");
Smartphone iphone = new Iphone("987654321", "IPhone 13", "222222222222222", 128000);
iphone.ReceberLigacao();
iphone.InstalarAplicativo("Telegram");