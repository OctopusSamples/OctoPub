import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Errors, Product} from "../model/Product";
import {useNavigate, useParams} from "react-router-dom";
import {styles} from "../utils/styles";
import {getJsonApi, isBranchingEnabled, patchJsonApi} from "../utils/network";
import {getAccessToken} from "../utils/security";

const UpdateBook: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const {bookId} = useParams();
    const history = useNavigate();
    const context = useContext(AppContext);
    const classes = styles();
    const [disabled, setDisabled] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [book, setBook] = useState<Product | null>(null);

    context.setAllBookId(null);

    const accessToken = getAccessToken();

    useEffect(() => {
        getJsonApi<Product|Errors>(context.settings.productEndpoint + "/" + bookId, context.partition, accessToken)
            .then(data => {
                if ("data" in data) {
                    const product = data as Product;
                    setBook(product);

                    if (!accessToken) {
                        setError("You must log into the app to update a book. Open the settings to perform a login.");
                    } else if (product?.data?.attributes?.dataPartition !== context.partition) {
                        setError("This book belongs to the "
                            + product?.data?.attributes?.dataPartition
                            + " data partition, and cannot be edited.");
                    } else {
                        setDisabled(false);
                    }
                }
                if ("errors" in data) {
                    const error = data as Errors;
                    setError(error.errors[0].title || "Failed to load book."
                        + (isBranchingEnabled() ? " Branching rules are enabled - double check they are valid, or disable them." : ""));
                }
            })
            .catch(() => {
                setError("There was an error retrieving the resource.");
                setBook(null);
            });
    }, [setBook, accessToken, setDisabled, context.partition, context.settings.productEndpoint, bookId]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>

            <Grid container={true}>
                {book && book.data && book.data.attributes &&
                    <>
                        <Grid container={true} className={classes.cell} md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>Id</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"id"} disabled={true} fullWidth={true} variant={"outlined"}
                                       value={book.data.id}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>Name</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"name"} disabled={disabled} fullWidth={true} variant={"outlined"}
                                       value={book.data.attributes.name}
                                       onChange={v => updateBook(v, "name")}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>Image</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"image"} disabled={disabled} fullWidth={true} variant={"outlined"}
                                       value={book.data.attributes.image}
                                       onChange={v => updateBook(v, "image")}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>EPUB</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"epub"} disabled={disabled} fullWidth={true} variant={"outlined"}
                                       value={book.data.attributes.epub}
                                       onChange={v => updateBook(v, "epub")}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>PDF</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"pdf"} disabled={disabled} fullWidth={true} variant={"outlined"}
                                       value={book.data.attributes.pdf}
                                       onChange={v => updateBook(v, "pdf")}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>Description</FormLabel>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id={"description"} disabled={disabled} multiline={true} rows={10}
                                       fullWidth={true}
                                       variant={"outlined"}
                                       value={book.data.attributes.description}
                                       onChange={v => updateBook(v, "description")}/>
                        </Grid>
                        <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                        </Grid>
                        <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                            <Button variant={"outlined"} disabled={disabled} onClick={_ => saveBook()}>Update
                                Book</Button>
                        </Grid>
                    </>}
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function updateBook(input: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>, property: string) {
        if (book) {
            setBook({
                data: {
                    id: null,
                    type: book.data.type,
                    attributes: {...book.data.attributes, [property]: input.target.value}
                }
            })
        }
    }

    function saveBook() {
        setDisabled(true);
        patchJsonApi(JSON.stringify(book, (key, value) => {
                if (value !== null) return value
            }),
            context.settings.productEndpoint + "/" + bookId,
            context.partition,
            getAccessToken())
            .then(_ => history('/index.html'))
            .catch(_ => {
                setDisabled(false);
                setError("An error occurred and the book was not updated.");
            });
    }
}


export default UpdateBook;