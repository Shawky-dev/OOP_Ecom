import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import { useNavigate } from 'react-router-dom';


const priceTagStyle = {
    display: 'inline-block',
    width: 'auto',
    height: '38px',
    backgroundColor: '#6ab070',
    borderRadius: '3px 4px 4px 3px',
    borderLeft: '1px solid #6ab070',
    marginLeft: '19px',
    position: 'relative',
    color: 'white',
    fontWeight: '300',
    fontSize: '18px',
    lineHeight: '38px',
    padding: '0 10px 0 10px'
}

function ItemCard(props) {

    const navigate = useNavigate();

    const getUserID = () => {
        return localStorage.getItem('id');
    };
    const getUserRole = () => {
        return localStorage.getItem('role');
    };

    const deleteItem = async (id) => {
        console.log(id);
        try {
            const response = await fetch(`http://localhost:8080/items/${id}`, {
                method: 'DELETE',

            });
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            console.log('Item deleted successfully');
            props.onDelete(); // Call the onDelete function passed down from the App component
        } catch (error) {
            console.error('There was a problem with the fetch operation:', error);
        }
    };
    const addItemToCart = async (id) =>{
        try {
            const response = await fetch(`http://localhost:8080/cart/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                  },
                  body: JSON.stringify({
                    userID : getUserID(),
                    method :"add"
                  }),
            });
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            console.log(`Item ${id} added to cart successfully`);
            props.onDelete(); // Call the onDelete function passed down from the App component
        } catch (error) {
            console.error('There was a problem with the fetch operation:', error);
        }

    }

    return (
        <>
            <Card style={{ width: '14rem', height:"28rem", marginBottom:"40px", backgroundColor:'#FFF7FC', display: 'flex', flexDirection: 'column' }}>
                <Card.Img variant="top" src={props.ImageBase64} width={"100px"} height={"200px"} />
                <Card.Body style={{ flex: '1 0 auto' }}>
                    <div style={{ display: "flex", justifyContent: "space-between" }}>
                        <div>
                            <Card.Title>{props.title}</Card.Title>
                            <Card.Title style={{ fontSize: "14px", opacity: "50%" }}>{props.Category}</Card.Title>
                        </div>
                        <Card.Title style={priceTagStyle}>{props.Price}</Card.Title>
                    </div>
                    <Card.Text>
                        {props.text}
                    </Card.Text>
                </Card.Body>
                <div style={{ marginBottom: '10px', marginLeft: '10px', marginRight: '10px' }}>
                    {getUserRole() === 'admin' ? (
                        <>
                        <Button size="sm" variant='danger' onClick={() => deleteItem(props.ID)}>Delete Item</Button>
                        <Button size="sm" variant='warning' onClick={() => navigate(`/edititem/${props.ID}`)}>Edit Item</Button>
                       </>
                    ) : (
                        
                        <Button size="sm" variant="primary" onClick={() =>addItemToCart(props.ID)}>add to Cart</Button>
                    )}
                </div>
            </Card>
        </>
    );
}

export default ItemCard;
