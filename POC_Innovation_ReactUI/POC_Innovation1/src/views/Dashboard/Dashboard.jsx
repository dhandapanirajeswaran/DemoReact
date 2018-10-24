import React, { Component } from "react";
import ChartistGraph from "react-chartist";
import { Grid, Row, Col,Table } from "react-bootstrap";
import { thArray, tdArray } from "variables/Variables.jsx";
import { Card } from "components/Card/Card.jsx";
import { StatsCard } from "components/StatsCard/StatsCard.jsx";

import {
  dataPie,
  legendPie,
  dataSales,
  optionsSales,
  responsiveSales,
  legendSales,
  dataBar,
  optionsBar,
  responsiveBar,
  legendBar
} from "variables/Variables.jsx";

import {Bar} from 'react-chartjs-2';
 
const data = {
    labels: ['Apple', 'Banana', 'Orange', 'Grape'],
    datasets: [
      {
        label: 'Inventory',
        backgroundColor: 'lightblue',
        borderColor: 'rgba(255,99,132,1)',
        borderWidth: 1,
        hoverBackgroundColor: 'rgba(255,99,132,0.4)',
        hoverBorderColor: 'rgba(255,99,132,1)',
        data: [65, 59, 80, 81]
      }
    ]
  };

class Dashboard extends Component {
  constructor(props) {
    super(props);
    this.state = {
      products: []
    };
  }
  componentDidMount() {
  
    this.interval = setInterval(() => {
        fetch('https://localhost:5001/api/product')
        .then(response => response.json())
        .then(data => this.setState({products: data}),  () => console.log(this.state.products));
      }, 2000);

    
  }
  createLegend(json) {
    var legend = [];
    for (var i = 0; i < json["names"].length; i++) {
      var type = "fa fa-circle text-" + json["types"][i];
      legend.push(<i className={type} key={i} />);
      legend.push(" ");
      legend.push(json["names"][i]);
    }
    return legend;
  }
  render() {
    return (
      <div className="content">
        <Grid fluid>
          <Row>
            <Col lg={3} sm={6}>
              <StatsCard
                bigIcon={<i className="pe-7s-server text-warning" />}
                statsText="Total SKUs in Inventory"
                statsValue={this.state.products[1]}
                statsIcon={<i className="fa fa-refresh" />}
           
              />
            </Col>
            <Col lg={3} sm={6}>
              <StatsCard
                bigIcon={<i className="pe-7s-wallet text-success" />}
                statsText="Total SKUs below Threshold"
                statsValue={this.state.products[0]}
                statsIcon={<i className="fa fa-calendar-o" />}
              
              />
            </Col>
            <Col lg={3} sm={6}>    
               {this.state.products[2]<75 &&
              <StatsCard
                bigIcon={<i className="pe-7s-bottom-arrow text-info" />}
                statsText="Total Inventory Availability in %"
                statsValue={this.state.products[2]}
                statsIcon={<i className="fa fa-clock-o" />}          
              />
               }
              {this.state.products[2]>=75 &&
                <StatsCard
                bigIcon={<i className="pe-7s-up-arrow text-info" />}
                statsText="Total Inventory Availability in %"
                statsValue={this.state.products[2]}
                statsIcon={<i className="fa fa-clock-o" />}          
              />   
               }
            </Col>
            {/* <Col lg={3} sm={6}>
              <StatsCard
                bigIcon={<i className="fa fa-twitter text-info" />}
                statsText="Followers"
                statsValue="+45"
                statsIcon={<i className="fa fa-refresh" />}
                statsIconText="Updated now"
              />
            </Col> */}
          </Row>
          <Row>
              <Col >                          
                    <Bar
                        data={data}
                        width={100}
                        height={500}
                        options={{
                            maintainAspectRatio: false
                        }}
                      />
            </Col>
          </Row>
                          
        </Grid>
              
      </div>      

    );
  }
}

export default Dashboard;
