import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useNavigate} from "react-router-dom";
import {styles} from "../utils/styles";
import {postJsonApi} from "../utils/network";
import {getAccessToken} from "../utils/security";

const AddBook: FC<CommonProps> = (props: CommonProps): ReactElement => {


    const history = useNavigate();
    const context = useContext(AppContext);
    const classes = styles();
    const [disabled, setDisabled] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [book, setBook] = useState<Product>({
        data: {
            id: null,
            type: "products",
            attributes: {
                name: "",
                description: "",
                image: "",
                epub: "",
                pdf: "",
                dataPartition: ""
            }
        }
    });

    context.setAllBookId(null);

    const accessTokenExists = !!getAccessToken();

    useEffect(() => {
        if (!accessTokenExists) {
            setError("The API key must be defined in the settings page.");
        } else {
            setDisabled(false);
        }
    }, [setDisabled, setError, accessTokenExists]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
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
                    <TextField id={"description"} disabled={disabled} multiline={true} rows={10} fullWidth={true}
                               variant={"outlined"}
                               value={book.data.attributes.description}
                               onChange={v => updateBook(v, "description")}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <Button variant={"outlined"} disabled={disabled} onClick={_ => saveBook()}>Create New Book</Button>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function updateBook(input: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>, property: string) {
        setBook({
            data: {
                id: null,
                type: book.data.type,
                attributes: {...book.data.attributes, [property]: input.target.value}
            }
        })
    }

    function saveBook() {
        setDisabled(true);
        postJsonApi(JSON.stringify(book, (key, value) => {
                if (value !== null) return value
            }),
            context.settings.productEndpoint,
            context.partition,
            getAccessToken())
            .then(_ => history('/index.html'))
            .catch(_ => {
                setDisabled(false);
                setError("An error occurred and the book was not saved.");
            });
    }
}


export default AddBook;