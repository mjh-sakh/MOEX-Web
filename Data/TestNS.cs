namespace BlazorApp.TestNS
{

    public class ItWorks
    {
        public string Greeting {get; private set;}
        public ItWorks(string name){
            Greeting = $"Hello {name}";
        }
    }

}