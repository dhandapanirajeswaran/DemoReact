import Sales from "views/Sales/Sales";
import Product from "views/Product/Product";
import Dashboard from "views/Dashboard/Dashboard";
const dashboardRoutes = [
  {
    path: "/sales",
    name: "Sales",
    icon: "pe-7s-graph",
    component: Sales
  },
  
  {
    path: "/product",
    name: "Products",
    icon: "pe-7s-note2",
    component: Dashboard
   },

  { redirect: true, path: "/", to: "/sales", name: "Sales" }
];

export default dashboardRoutes;
