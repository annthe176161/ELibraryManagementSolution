import requests
import json

# Test API directly
def test_api():
    base_url = "https://localhost:7125"
    
    # Disable SSL verification for development
    import urllib3
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
    
    # Test available books endpoint
    try:
        response = requests.get(f"{base_url}/api/Book/available", verify=False)
        print(f"Status: {response.status_code}")
        print(f"Response: {response.text[:500]}...")
        
        if response.status_code == 200:
            data = response.json()
            print(f"Number of books returned: {len(data) if isinstance(data, list) else 'Not a list'}")
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_api()