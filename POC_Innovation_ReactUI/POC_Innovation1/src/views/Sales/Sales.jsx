import React, { Component } from "react";
import { Grid, Row, Col,Table } from "react-bootstrap";
import { thSalesArray } from "variables/Variables.jsx";
import { Card } from "components/Card/Card.jsx";
class Sales extends Component {
  constructor(props) {
    super(props);
    this.state = {
      sales: []
    };
  }

  componentDidMount() {
    this.interval = setInterval(() => {
      fetch('https://localhost:6001/api/sales')
      .then(response => response.json())
      .then(data => this.setState({sales: data}),()=>console.log(this.state.sales));
    }, 2000);
  }
  
  componentWillUnmount() {
    clearInterval(this.interval);
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
            <Col md={12}>
              <Card
                ctTableFullWidth
                ctTableResponsive
                content={
                  <Table striped hover>
                    <thead>
                      <tr>
                        {thSalesArray.map((prop, key) => {
                          return <th key={key}>{prop}</th>;
                        })}
                      </tr>
                    </thead>
                    <tbody>
                    {this.state.sales.map((prop, key) => {
                      return (
                        <tr key={key}>
                          {Object.keys(prop).map((item, key) => {
                            return  <td key={key}>{prop[item]}</td>
                          })}
                        </tr>
                      );
                    })}
                  </tbody>
                  </Table>
                }
              />
            </Col>
            </Row>
        </Grid>
      </div>
    );
  }
}

export default Sales;
