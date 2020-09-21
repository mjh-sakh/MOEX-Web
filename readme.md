# About
It is my study-project to learn C# and a bit of Web-Dev. 
- [x] наследование классов 
- [ ] интерфейсы
- [x] extension классы 
- [ ] generics
- [ ] документация 
- [x] overloads
- [x] unit tests
- [x] asynchronous tasks, parallel runs 
- [x] working with browser local storage

# Service 
Service allows to build portfolio of stocks and requests data from MOEX to analyze how value of it changed in requested period. 
Balancing interval may be provided. In this case original proportion between stocks will be restored at the end of each interval. 

# Use
Add each stock to the table, then hit 'get data' and output is provided in very short format: 
- initial value
- final value
- change in % 

![snapshot](snapshot.jpg)

# Plans
Above is just a foundation to get data from MOEX. Next intentions are:
- [x] balancing with given frequency and fee
- [ ] balancing with given fee
- [ ] running and comparing multiple portfolio
- [x] web interface 
- [ ] classic graphs (starting at 100% and then ups and downs)
- [x] saving and loading a portfolio
- [ ] save several portfolios and load selected 
- [x] move to client side app (wasp)


## minor features
- [x] work day check or bypass so no empty data received from MOEX
- [x] change default dates to last work day and one year prior 
