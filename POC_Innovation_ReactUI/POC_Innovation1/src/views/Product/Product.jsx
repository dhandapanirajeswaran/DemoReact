import React, { Component } from "react";
import { Grid, Row, Col, Table } from "react-bootstrap";

import Card from "components/Card/Card.jsx";
import { thArray, tdArray } from "variables/Variables.jsx";

import BarChart from 'react-d3-components'


var data = [{
    label: 'somethingA',
    values: [{x: 'SomethingA', y: 10}, {x: 'SomethingB', y: 4}, {x: 'SomethingC', y: 3}]
}];

class Product extends Component {
  constructor(props) {
    super(props);
    this.state = {
      products: []
    };
  }

  componentDidMount() {    
    this.interval = setInterval(() => {
      fetch('https://localhost:6001/api/product')
    .then(response => response.json())
    .then(data => this.setState({products: data}),  () => console.log(this.state.products));
    }, 2000);
  }
  
  componentWillUnmount() {
    clearInterval(this.interval);
  }
  
  render() {
    return (
      <div className="content">
      
      </div>
    );
  }
}

export default Product;
