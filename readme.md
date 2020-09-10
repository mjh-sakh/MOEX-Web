# About
Simple script to request data from MOEX to analyze how given portfolio changed in requested period. 

# Use
Add each stock to the table, then hit 'get data' and output is provided in very short format in console: 
- initial value
- final value
- change in % 

![snapshot](snapshot.jpg)

# Plans
Above is just a foundation to get data from MOEX. Next intentions are:
- [ ] balancing with given frequency and fee
- [ ] running and comparing multiple portfolio
- [ ] data export per stock
- [x] web interface 
- [ ] classic graphs (starting at 100% and then ups and downs)
- [ ] saving portfolios 


## minor features
- work day check or bypass so no empty data received from MOEX
- change default dates to last work day and one year prior 
