import React, { useEffect, useState } from "react";
import { Navbar, Nav, NavDropdown, Form, FormControl, Button } from 'react-bootstrap';
import Image from 'react-bootstrap/Image';
import { useNavigate } from 'react-router-dom'; // Import useNavigate

const AdminNav = ({id}) => {
 console.log(id)
 const [items, setItems] = useState(null);
 const navigate = useNavigate(); // Use the useNavigate hook

 useEffect(() => {
    fetch('http://localhost:8080/admins/'+id)
        .then(response => response.json())
        .then(data => {
          setItems(data)
          console.log(data)
        })
        .catch(error => console.error('Error:', error));
 }, [id]); // Added id to the dependency array to refetch if id changes

 // Function to handle the logout action
 const handleLogout = () => {
    // Clear local storage
    localStorage.clear();
    // Navigate to the /login route
    navigate('/login');
 };

 // Conditional rendering to check if items is not null
 return (
    <Nav className="ml-auto" style={{ marginRight: '20px' }}>
      {items && (
        <>
          <Image src={items.ProfilePictureBase64} roundedCircle width={"50px"}/>
        <NavDropdown title={items.Name} id="basic-nav-dropdown" drop="down-centered">
          <NavDropdown.Item onClick={()=> navigate('/settings')}>Settings</NavDropdown.Item>
          <NavDropdown.Item onClick={()=> navigate('/usershistory')}>User Activites</NavDropdown.Item>
          <NavDropdown.Item onClick={handleLogout}>LogOut</NavDropdown.Item> {/* Updated to use onClick */}
        </NavDropdown>
        </>
      )}
      <Button onClick={() => navigate("/additem")} variant="outline-success" className="ml-auto" style={{ marginRight: '20px' }}>AddItem</Button>
    </Nav>
 )
}

export default AdminNav;
