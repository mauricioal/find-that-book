import { useState } from 'react';
import { Container, Row, Col, Form, Button, Card, Spinner, Alert, Badge } from 'react-bootstrap';
import { Search, BookOpen, User, Calendar, ExternalLink } from 'lucide-react';
import type { BookCandidate } from './types';
import { CONFIG } from './config/constants';

function App() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<BookCandidate[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searched, setSearched] = useState(false);

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!query.trim()) return;

    setLoading(true);
    setError(null);
    setResults([]);
    setSearched(true);

    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/search?query=${encodeURIComponent(query)}`);
      
      if (!response.ok) {
        throw new Error('Failed to fetch results. Please try again.');
      }

      const data = await response.json();
      setResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 bg-light py-5">
      <Container>
        <div className="text-center mb-5">
          <h1 className="display-4 fw-bold mb-3 text-primary">
            <BookOpen className="me-2" size={48} style={{ marginTop: -10 }} />
            Find That Book
          </h1>
          <p className="lead text-muted">
            Describe a book (title, author, or keywords) and we'll find it for you using AI.
          </p>
        </div>

        <Row className="justify-content-center mb-5">
          <Col md={8} lg={6}>
            <Form onSubmit={handleSearch} className="d-flex shadow-sm rounded-pill p-1 bg-white">
              <Form.Control
                type="text"
                placeholder="e.g. tolkien hobbit illustrated..."
                className="border-0 rounded-pill shadow-none px-4 py-3"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                autoFocus
              />
              <Button 
                variant="primary" 
                type="submit" 
                className="rounded-pill px-4 m-1 fw-bold d-flex align-items-center"
                disabled={loading}
              >
                {loading ? <Spinner size="sm" animation="border" /> : <Search size={20} />}
              </Button>
            </Form>
          </Col>
        </Row>

        {error && (
          <Row className="justify-content-center">
            <Col md={8}>
              <Alert variant="danger">{error}</Alert>
            </Col>
          </Row>
        )}

        {searched && !loading && results.length === 0 && !error && (
          <div className="text-center text-muted mt-5">
            <p>No books found. Try a different query.</p>
          </div>
        )}

        <Row className="g-4">
          {results.map((book, index) => (
            <Col key={index} xs={12} lg={6}>
              <Card className="h-100 border-0 shadow-sm hover-shadow transition-all">
                <Card.Body className="d-flex gap-4">
                  <div className="flex-shrink-0" style={{ width: '100px' }}>
                    {book.coverUrl ? (
                      <img 
                        src={book.coverUrl} 
                        alt={book.title} 
                        className="w-100 rounded shadow-sm"
                        style={{ objectFit: 'cover', aspectRatio: '2/3' }}
                      />
                    ) : (
                      <div 
                        className="bg-secondary bg-opacity-10 rounded d-flex align-items-center justify-content-center text-muted"
                        style={{ width: '100px', aspectRatio: '2/3' }}
                      >
                        <BookOpen size={32} />
                      </div>
                    )}
                  </div>
                  
                  <div className="flex-grow-1">
                    <div className="d-flex justify-content-between align-items-start mb-2">
                      <h4 className="h5 fw-bold mb-0 text-dark">{book.title}</h4>
                      {index === 0 && <Badge bg="success">Top Match</Badge>}
                    </div>

                    <div className="d-flex flex-wrap gap-3 text-muted small mb-3">
                      <div className="d-flex align-items-center">
                        <User size={14} className="me-1" />
                        {book.authors.join(', ') || 'Unknown Author'}
                      </div>
                      {book.firstPublishYear && (
                        <div className="d-flex align-items-center">
                          <Calendar size={14} className="me-1" />
                          {book.firstPublishYear}
                        </div>
                      )}
                    </div>

                    <div className="bg-primary bg-opacity-10 p-3 rounded mb-3">
                      <p className="mb-0 small text-primary-emphasis">
                        <strong>Why:</strong> {book.explanation}
                      </p>
                    </div>

                    <a 
                      href={`${CONFIG.OPENLIBRARY_URL}${book.openLibraryId}`} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="btn btn-sm btn-outline-secondary d-inline-flex align-items-center"
                    >
                      View on OpenLibrary <ExternalLink size={12} className="ms-1" />
                    </a>
                  </div>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      </Container>
    </div>
  );
}

export default App;